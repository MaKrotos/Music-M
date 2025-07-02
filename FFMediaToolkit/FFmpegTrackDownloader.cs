using FFmpeg.AutoGen;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FFMediaToolkit
{
    public static class FFmpegTrackDownloader
    {
        public static unsafe Task DownloadAndConvertWithFFmpegAutogen(string inputUrl, string outputPath)
        {
            return Task.Run(() =>
            {
                // Регистрируем все кодеки и форматы
                ffmpeg.avformat_network_init();

                AVFormatContext* pFormatContext = null;
                AVFormatContext* pOutputFormatContext = null;

                try
                {
                    // Открываем входной поток
                    if (ffmpeg.avformat_open_input(&pFormatContext, inputUrl, null, null) != 0)
                    {
                        throw new Exception("Could not open input file.");
                    }

                    // Получаем информацию о потоке
                    if (ffmpeg.avformat_find_stream_info(pFormatContext, null) < 0)
                    {
                        throw new Exception("Could not find stream information.");
                    }

                    // Находим аудио поток
                    int audioStreamIndex = -1;
                    for (int i = 0; i < pFormatContext->nb_streams; i++)
                    {
                        if (pFormatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                        {
                            audioStreamIndex = i;
                            break;
                        }
                    }

                    if (audioStreamIndex == -1)
                    {
                        throw new Exception("Could not find audio stream.");
                    }

                    // Получаем параметры кодека
                    AVCodecParameters* pCodecParameters = pFormatContext->streams[audioStreamIndex]->codecpar;
                    AVCodec* pCodec = ffmpeg.avcodec_find_decoder(pCodecParameters->codec_id);

                    if (pCodec == null)
                    {
                        throw new Exception("Unsupported codec.");
                    }

                    // Создаем контекст кодека
                    AVCodecContext* pCodecContext = ffmpeg.avcodec_alloc_context3(pCodec);
                    if (ffmpeg.avcodec_parameters_to_context(pCodecContext, pCodecParameters) < 0)
                    {
                        throw new Exception("Could not copy codec parameters to context.");
                    }

                    // Открываем кодек
                    if (ffmpeg.avcodec_open2(pCodecContext, pCodec, null) < 0)
                    {
                        throw new Exception("Could not open codec.");
                    }

                    // Создаем контекст выходного файла
                    if (ffmpeg.avformat_alloc_output_context2(&pOutputFormatContext, null, null, outputPath) < 0)
                    {
                        throw new Exception("Could not create output context.");
                    }

                    // Создаем выходной поток
                    AVStream* pOutputStream = ffmpeg.avformat_new_stream(pOutputFormatContext, null);
                    if (pOutputStream == null)
                    {
                        throw new Exception("Could not create output stream.");
                    }

                    // Устанавливаем параметры кодека для выходного потока
                    if (ffmpeg.avcodec_parameters_copy(pOutputStream->codecpar, pCodecParameters) < 0)
                    {
                        throw new Exception("Could not copy codec parameters.");
                    }

                    // Открываем выходной файл
                    if ((ffmpeg.avio_open(&pOutputFormatContext->pb, outputPath, ffmpeg.AVIO_FLAG_WRITE) < 0))
                    {
                        throw new Exception("Could not open output file.");
                    }

                    // Записываем заголовок файла
                    if (ffmpeg.avformat_write_header(pOutputFormatContext, null) < 0)
                    {
                        throw new Exception("Could not write header.");
                    }

                    // Читаем и записываем пакеты
                    AVPacket* pPacket = ffmpeg.av_packet_alloc();
                    AVFrame* pFrame = ffmpeg.av_frame_alloc();

                    while (ffmpeg.av_read_frame(pFormatContext, pPacket) >= 0)
                    {
                        if (pPacket->stream_index == audioStreamIndex)
                        {
                            // Перемещаем пакет в выходной поток
                            pPacket->stream_index = pOutputStream->index;

                            // Пересчитываем временные метки
                            ffmpeg.av_packet_rescale_ts(pPacket,
                                pFormatContext->streams[audioStreamIndex]->time_base,
                                pOutputStream->time_base);

                            // Записываем пакет
                            if (ffmpeg.av_interleaved_write_frame(pOutputFormatContext, pPacket) < 0)
                            {
                                throw new Exception("Error while writing packet.");
                            }
                        }

                        ffmpeg.av_packet_unref(pPacket);
                    }

                    // Записываем завершающую часть файла
                    ffmpeg.av_write_trailer(pOutputFormatContext);
                }
                finally
                {
                    // Освобождаем ресурсы
                    if (pFormatContext != null)
                    {
                        ffmpeg.avformat_close_input(&pFormatContext);
                    }
                    if (pOutputFormatContext != null && pOutputFormatContext->pb != null)
                    {
                        ffmpeg.avio_closep(&pOutputFormatContext->pb);
                    }
                    if (pOutputFormatContext != null)
                    {
                        ffmpeg.avformat_free_context(pOutputFormatContext);
                    }
                    ffmpeg.avformat_network_deinit();
                }
            });
        }

        public static unsafe Task DownloadAndConvertWithFFmpegAutogen(string inputUrl, string outputPath, IProgress<(long done, long total)> progress)
        {
            return Task.Run(() =>
            {
                ffmpeg.avformat_network_init();

                AVFormatContext* pFormatContext = null;
                AVFormatContext* pOutputFormatContext = null;

                try
                {
                    if (ffmpeg.avformat_open_input(&pFormatContext, inputUrl, null, null) != 0)
                    {
                        throw new Exception("Could not open input file.");
                    }

                    if (ffmpeg.avformat_find_stream_info(pFormatContext, null) < 0)
                    {
                        throw new Exception("Could not find stream information.");
                    }

                    int audioStreamIndex = -1;
                    for (int i = 0; i < pFormatContext->nb_streams; i++)
                    {
                        if (pFormatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                        {
                            audioStreamIndex = i;
                            break;
                        }
                    }

                    if (audioStreamIndex == -1)
                    {
                        throw new Exception("Could not find audio stream.");
                    }

                    AVCodecParameters* pCodecParameters = pFormatContext->streams[audioStreamIndex]->codecpar;
                    AVCodec* pCodec = ffmpeg.avcodec_find_decoder(pCodecParameters->codec_id);

                    if (pCodec == null)
                    {
                        throw new Exception("Unsupported codec.");
                    }

                    AVCodecContext* pCodecContext = ffmpeg.avcodec_alloc_context3(pCodec);
                    if (ffmpeg.avcodec_parameters_to_context(pCodecContext, pCodecParameters) < 0)
                    {
                        throw new Exception("Could not copy codec parameters to context.");
                    }

                    if (ffmpeg.avcodec_open2(pCodecContext, pCodec, null) < 0)
                    {
                        throw new Exception("Could not open codec.");
                    }

                    if (ffmpeg.avformat_alloc_output_context2(&pOutputFormatContext, null, null, outputPath) < 0)
                    {
                        throw new Exception("Could not create output context.");
                    }

                    AVStream* pOutputStream = ffmpeg.avformat_new_stream(pOutputFormatContext, null);
                    if (pOutputStream == null)
                    {
                        throw new Exception("Could not create output stream.");
                    }

                    if (ffmpeg.avcodec_parameters_copy(pOutputStream->codecpar, pCodecParameters) < 0)
                    {
                        throw new Exception("Could not copy codec parameters.");
                    }

                    if ((ffmpeg.avio_open(&pOutputFormatContext->pb, outputPath, ffmpeg.AVIO_FLAG_WRITE) < 0))
                    {
                        throw new Exception("Could not open output file.");
                    }

                    if (ffmpeg.avformat_write_header(pOutputFormatContext, null) < 0)
                    {
                        throw new Exception("Could not write header.");
                    }

                    AVPacket* pPacket = ffmpeg.av_packet_alloc();
                    AVFrame* pFrame = ffmpeg.av_frame_alloc();

                    // Получаем общий размер аудиопотока (в байтах)
                    long totalBytes = 0;
                    long doneBytes = 0;
                    long streamDuration = pFormatContext->streams[audioStreamIndex]->duration;
                    AVRational timeBase = pFormatContext->streams[audioStreamIndex]->time_base;
                    long bitRate = pCodecParameters->bit_rate;

                    if (streamDuration > 0 && bitRate > 0)
                    {
                        // duration в time_base, bitrate в битах/сек, переводим в байты
                        double seconds = streamDuration * ffmpeg.av_q2d(timeBase);
                        totalBytes = (long)(bitRate / 8.0 * seconds);
                    }
                    else
                    {
                        totalBytes = 0; // Не удалось определить
                    }

                    while (ffmpeg.av_read_frame(pFormatContext, pPacket) >= 0)
                    {
                        if (pPacket->stream_index == audioStreamIndex)
                        {
                            pPacket->stream_index = pOutputStream->index;
                            ffmpeg.av_packet_rescale_ts(pPacket,
                                pFormatContext->streams[audioStreamIndex]->time_base,
                                pOutputStream->time_base);
                            if (ffmpeg.av_interleaved_write_frame(pOutputFormatContext, pPacket) < 0)
                            {
                                throw new Exception("Error while writing packet.");
                            }
                            doneBytes += pPacket->size;
                            progress?.Report((doneBytes, totalBytes));
                        }
                        ffmpeg.av_packet_unref(pPacket);
                    }

                    ffmpeg.av_write_trailer(pOutputFormatContext);
                }
                finally
                {
                    if (pFormatContext != null)
                    {
                        ffmpeg.avformat_close_input(&pFormatContext);
                    }
                    if (pOutputFormatContext != null && pOutputFormatContext->pb != null)
                    {
                        ffmpeg.avio_closep(&pOutputFormatContext->pb);
                    }
                    if (pOutputFormatContext != null)
                    {
                        ffmpeg.avformat_free_context(pOutputFormatContext);
                    }
                    ffmpeg.avformat_network_deinit();
                }
            });
        }
    }
} 