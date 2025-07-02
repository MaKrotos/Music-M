using System;
using FFMediaToolkit.Helpers;
using FFmpeg.AutoGen;
using System.Runtime.InteropServices;

namespace FFMediaToolkit.Decoding.Internal
{
    internal unsafe class AudioEqualizerFilter : IDisposable
    {
        private AVFilterGraph* filterGraph;
        private AVFilterContext* buffersrcCtx;
        private AVFilterContext* buffersinkCtx;

        public AudioEqualizerFilter(AVCodecContext* codecCtx, string eqArgs)
        {
            filterGraph = ffmpeg.avfilter_graph_alloc();

            string sampleFmtName = ffmpeg.av_get_sample_fmt_name(codecCtx->sample_fmt) ?? "fltp";

            byte[] layoutBuffer = new byte[64];
            string channelLayoutStr;
            fixed (byte* layoutPtr = layoutBuffer)
            {
                ffmpeg.av_channel_layout_describe(&codecCtx->ch_layout, layoutPtr, (ulong)layoutBuffer.Length);
                channelLayoutStr = Marshal.PtrToStringAnsi((IntPtr)layoutPtr);
            }

            string abufferArgs = $"sample_rate={codecCtx->sample_rate}:sample_fmt={sampleFmtName}:channels={codecCtx->ch_layout.nb_channels}:channel_layout={channelLayoutStr}";
            AVFilter* abuffer = ffmpeg.avfilter_get_by_name("abuffer");
            if (abuffer == null)
                throw new Exception("FFmpeg: Фильтр 'abuffer' не найден!");
            fixed (AVFilterContext** pBuffersrcCtx = &buffersrcCtx)
            {
                int abufRes = ffmpeg.avfilter_graph_create_filter(pBuffersrcCtx, abuffer, "in", abufferArgs, null, filterGraph);
                System.Diagnostics.Debug.WriteLine($"[FFMedia] abufferArgs: {abufferArgs}, abufRes={abufRes}, ctx==null? {(*pBuffersrcCtx == null)}");
                if (abufRes < 0 || *pBuffersrcCtx == null)
                    throw new Exception($"Не удалось создать фильтр abuffer. Код ошибки: {abufRes}, строка: {abufferArgs}");
            }

            AVFilter* abuffersink = ffmpeg.avfilter_get_by_name("abuffersink");
            if (abuffersink == null)
                throw new Exception("FFmpeg: Фильтр 'abuffersink' не найден!");
            fixed (AVFilterContext** pBuffersinkCtx = &buffersinkCtx)
            {
                int absinkRes = ffmpeg.avfilter_graph_create_filter(pBuffersinkCtx, abuffersink, "out", null, null, filterGraph);
                System.Diagnostics.Debug.WriteLine($"[FFMedia] abuffersink, absinkRes={absinkRes}, ctx==null? {(*pBuffersinkCtx == null)}");
                if (absinkRes < 0 || *pBuffersinkCtx == null)
                    throw new Exception($"Не удалось создать фильтр abuffersink. Код ошибки: {absinkRes}");
            }

            // Разбиваем eqArgs на отдельные фильтры (ожидается строка вида 'equalizer=...,equalizer=...')
            var eqFilters = eqArgs.Split(new[] { ",equalizer=" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < eqFilters.Length; i++)
            {
                // Убираем возможный префикс 'equalizer=' у первого фильтра
                if (i == 0 && eqFilters[i].StartsWith("equalizer="))
                    eqFilters[i] = eqFilters[i].Substring("equalizer=".Length);
            }

            AVFilterContext* prevCtx = buffersrcCtx;
            AVFilterContext* eqCtx = null;
            for (int i = 0; i < eqFilters.Length; i++)
            {
                string singleEqArgs = eqFilters[i];
                AVFilter* eqFilter = ffmpeg.avfilter_get_by_name("equalizer");
                if (eqFilter == null)
                    throw new Exception("FFmpeg: Фильтр 'equalizer' не найден! Проверьте сборку FFmpeg.");
                System.Diagnostics.Debug.WriteLine($"[FFMedia] Попытка создать фильтр equalizer с параметрами: {singleEqArgs}");
                int eqRes = ffmpeg.avfilter_graph_create_filter(&eqCtx, eqFilter, $"eq{i}", singleEqArgs, null, filterGraph);
                System.Diagnostics.Debug.WriteLine($"[FFMedia] eqRes={eqRes}, eqCtx==null? {eqCtx == null}");
                if (eqRes < 0 || eqCtx == null)
                    throw new Exception($"Не удалось создать фильтр equalizer. Код ошибки: {eqRes}, строка: {singleEqArgs}");

                int link = ffmpeg.avfilter_link(prevCtx, 0, eqCtx, 0);
                if (link < 0)
                    throw new Exception($"Ошибка линковки фильтров. Код: {link}");
                prevCtx = eqCtx;
            }

            int link2 = ffmpeg.avfilter_link(eqCtx, 0, buffersinkCtx, 0);
            if (link2 < 0)
                throw new Exception($"Ошибка линковки eq->abuffersink. Код: {link2}");

            int graphRes = ffmpeg.avfilter_graph_config(filterGraph, null);
            if (graphRes < 0)
                throw new Exception($"Ошибка конфигурации графа фильтров. Код: {graphRes}");
        }

        public void SendFrame(AVFrame* frame)
        {
            ffmpeg.av_buffersrc_add_frame_flags(buffersrcCtx, frame, 0).ThrowIfError("send frame to filter");
        }

        public int ReceiveFrame(AVFrame* frame)
        {
            return ffmpeg.av_buffersink_get_frame(buffersinkCtx, frame);
        }

        public void Dispose()
        {
            if (filterGraph != null)
            {
                AVFilterGraph* graph = filterGraph;
                ffmpeg.avfilter_graph_free(&graph);
                filterGraph = null;
            }
        }
    }
} 