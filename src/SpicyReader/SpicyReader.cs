using OpenCvSharp;
using PcapngUtils.Common;
using PcapngUtils.PcapNG;
using System;
using System.Threading;

namespace SpicyReader
{
    public class SpicyReader
    {
        const int width = 1024;
        const int height = 1024;
        bool processingFrame = false;
        int lineCounter = 0;
        int frameCounter = 0;
        readonly ushort[,] integers = new ushort[height, width];
        bool secondHalfOfLine = false;
        //List<Mat> imageArray = new List<Mat>();

        public void OpenPcapNGFile(string filename, bool swapBytes, CancellationToken token)
        {
            using (var reader = new PcapNGReader(filename, swapBytes))
            {
                reader.OnReadPacketEvent += reader_OnReadPacketEvent;
                reader.ReadPackets(token);
            }
        }
        void reader_OnReadPacketEvent(object context, IPacket packet)
        {
            if (packet.Data.Length == 60)
            {
                //Start of frame packet
                if (processingFrame)
                {
                    Mat m = new Mat(width, height, MatType.CV_16U, integers);
                    Cv2.ImWrite(frameCounter.ToString() + ".tiff", m);
                    //imageArray.Add(m);
                    frameCounter++;
                }

                lineCounter = 0;
                processingFrame = true;
            }
            else
            {   // Line packet.
                if (processingFrame)
                {
                    int lineOffset = 0;
                    if (secondHalfOfLine)
                    {
                        lineOffset += 512;
                    }

                    var span = packet.Data.AsSpan();
                    for (int pixel = 0; pixel < 512; pixel++)
                    {
                        var ushortSpan = span.Slice(24 + (pixel * 2), 2);
                        ushortSpan.Reverse();
                        var number = BitConverter.ToUInt16(ushortSpan);
                        number *= 20;
                        integers[lineCounter, lineOffset + pixel] = number;
                    }

                    if(secondHalfOfLine)
                        lineCounter++;
                    secondHalfOfLine = !secondHalfOfLine;                   
                }
            }
        }
    }
}
