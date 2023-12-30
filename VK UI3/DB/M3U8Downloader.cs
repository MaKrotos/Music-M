namespace VK_UI3.DB
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using NAudio.Wave;
    using System.Security.Cryptography;
    using VkNet.Model;


    using System.Collections.Generic;
    using VkNet.Model.Attachments;

    public class M3U8Downloader
    {
        private string _m3u8Url;
        private string track_name;
        private Audio audio;
        private string m3u8FileContent = null;

        public M3U8Downloader(string m3u8Url, string track_name)
        {
            _m3u8Url = m3u8Url;
            this.track_name = track_name;
            DownloadM3U8File();
            ParseM3U8File();
        }





        private void DownloadM3U8File()
        {
            if (m3u8FileContent != null) { return; }
            using (var client = new WebClient())
            {
                m3u8FileContent = client.DownloadString(_m3u8Url);
            }
        }


        public class AudioSegment
        {
            public string Url { get; set; }
            public string KeyUrl { get; set; }
            public float segmentTime { get; set; }
            public int sq { get; set; }
            public int downloaded { get; set; }
            public byte[] dataSegment { get; set; }
        }

        int itemn = 1;
        List<AudioSegment> audioSegments = new List<AudioSegment> { };
        private List<AudioSegment> ParseM3U8File()
        {
            var lines = m3u8FileContent.Split('\n');
            string keyUrl = null;
            float timeTrack = 0;

            foreach (var line in lines)
            {
                if (line.StartsWith("#EXT-X-KEY:METHOD=AES-128"))
                {
                    var match = Regex.Match(line, "URI=\"(.*?)\"");
                    if (match.Success)
                    {
                        keyUrl = match.Groups[1].Value;
                    }
                }
                if (line.StartsWith("#EXTINF:"))
                {
                    var match = Regex.Match(line, "#EXTINF:\"(.*?)\",");
                    string linee = line.Replace("#EXTINF:", "");
                    linee = linee.Replace(",", "");
                    linee = linee.Replace(".", ",");
                    timeTrack = float.Parse(linee);

                }
                else if (line.StartsWith("#EXT-X-KEY:METHOD=NONE"))
                {
                    keyUrl = null;
                }
                else if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                {
                    audioSegments.Add(new AudioSegment
                    {
                        Url = line.Trim(),
                        KeyUrl = keyUrl,
                        sq = itemn++,
                        segmentTime = timeTrack
                    });

                }
            }
            return audioSegments;
        }


        public byte[] getDataSegment(int segmentNumber)
        {

            if (audioSegments.Count < segmentNumber) return null;
            if (audioSegments[segmentNumber].dataSegment != null) return audioSegments[segmentNumber].dataSegment;

            var audioData = DownloadAudio(_m3u8Url.Replace("index.m3u8", "") + audioSegments[segmentNumber].Url);
            if (audioSegments[segmentNumber].KeyUrl != null)
            {
                var lastKey = DownloadFileAndReadText(audioSegments[segmentNumber].KeyUrl);
                audioData = DecryptFile(audioData, lastKey, audioSegments[segmentNumber].sq);
            }
            return audioData;
        }




        public byte[] getDataSegment(AudioSegment segment)
        {
            if (segment.dataSegment != null) return segment.dataSegment;

            var audioData = DownloadAudio(_m3u8Url.Replace("index.m3u8", "") + segment.Url);
            if (segment.KeyUrl != null)
            {
                var lastKey = DownloadFileAndReadText(segment.KeyUrl);
                audioData = DecryptFile(audioData, lastKey, segment.sq);
            }
            return audioData;
        }

        Dictionary<string, string> keys = new Dictionary<string, string>();

        public string DownloadFileAndReadText(string url)
        {
            if (keys.ContainsKey(url)) return keys[url];
            using (WebClient client = new WebClient())
            {
                using (Stream stream = client.OpenRead(url))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string text = reader.ReadToEnd();
                        keys.Add(url, text);
                        return text;
                    }
                }
            }
        }

        public class segmentTime
        {
            public byte[] segment;
            public float timeinSegment;
            public int segmentNumber;
        }

        public segmentTime getSegmentByTime(float time)
        {
            ParseM3U8File();



            float totalDuration = 0;
            int segmentNumber = 0;
            var time_ost = time;

            foreach (var segment in audioSegments)
            {
                totalDuration += segment.segmentTime;

                if (totalDuration >= time)
                {
                    var sergment = new segmentTime();

                    var audioData = getDataSegment(segmentNumber);
                    sergment.segment = audioData;
                    sergment.timeinSegment = time_ost;
                    sergment.segmentNumber = segmentNumber;
                    return sergment;
                }
                time_ost -= segment.segmentTime;
                segmentNumber++;
            }

            return null;


        }

        public string CombineAudioFiles(string PathSave = null)
        {
            // getSegmentByTime(48);
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "segments", track_name);
            string pat;
            if (PathSave == null)
            {

                pat = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "segments");
            }
            else
                pat = PathSave;

            if (!Directory.Exists(pat)) Directory.CreateDirectory(pat);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            byte[] buffer = new byte[1024];

            using (WaveFileWriter waveFileWriter = new WaveFileWriter(pat + "\\" + track_name + ".mp3", new WaveFormat()))
            {

                foreach (var segment in audioSegments)
                {
                    var audioData = getDataSegment(segment);


                    using (FileStream fs = new FileStream(path + "\\" + segment.Url + ".mp3", FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(audioData, 0, audioData.Length);
                    }

                    using (MediaFoundationReader reader = new MediaFoundationReader(path + "\\" + segment.Url + ".mp3"))
                    {
                        int bytesRead;
                        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            waveFileWriter.Write(buffer, 0, bytesRead);
                        }

                    }
                    File.Delete(path + "\\" + segment.Url + ".mp3");
                }
                Directory.Delete(path);
            }

            return pat + "\\" + track_name + ".mp3";
        }


        private byte[] DownloadAudio(string url)
        {
            using (var client = new WebClient())
            {
                return client.DownloadData(url);
            }
        }


        public static byte[] tobigendianbytes(int i)
        {
            byte[] bytes = BitConverter.GetBytes(Convert.ToInt64(i));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }


        public static byte[] DecryptFile(byte[] cipherTextBytes, string key, int ivv)
        {
            byte[] plainTextBytes = null;

            using (Aes aes = Aes.Create())
            {

                byte[] iv = tobigendianbytes(ivv);
                iv = new byte[8].Concat(iv).ToArray();

                var keyBytes = new byte[16];
                var secretKeyBytes = Encoding.UTF8.GetBytes(key);
                Array.Copy(secretKeyBytes, keyBytes, Math.Min(keyBytes.Length, secretKeyBytes.Length));
                aes.Mode = CipherMode.CBC;
                aes.Key = keyBytes;

                aes.IV = iv; // AES needs a 16-byte IV

                // Create a decryptor
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(cipherTextBytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        plainTextBytes = new byte[cipherTextBytes.Length];

                        int decryptedByteCount = cs.Read(plainTextBytes, 0, plainTextBytes.Length);
                        ms.Close();
                        cs.Close();
                    }
                }
            }

            return plainTextBytes;
        }




    }
}
