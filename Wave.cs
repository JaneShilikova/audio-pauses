using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace wav
{
    class Wave
    {
        // chunk 0
        public char[] chunkID { private get; set; }
        public int fileSize { private get; set; }
        public char[] riffType { private get; set; }
        // chunk 1
        public char[] fmtID { private get; set; }
        public int fmtSize { get; set; } // bytes for this chunk
        public Int16 fmtCode { private get; set; }
        public Int16 channels { get; set; }
        public int sampleRate { /*private*/ get; set; }
        public int byteRate { private get; set; }
        public Int16 fmtBlockAlign { private get; set; }
        public Int16 bitDepth { get; set; }
        // Read any extra values
        public Int16 fmtExtraSize { get; set; }
        // chunk 2
        public char[] dataID { private get; set; }
        public int bytes { get; set; }
        // DATA!
        public byte[] byteArray { private get; set; }

        public int bytesForSamp { get; set; }
        public int samps { get; set; }

        
        public int duration()
        {
            int sec = this.bytes / this.bytesForSamp /
                this.channels / this.sampleRate;
            return sec;
        }
        public bool readWav(string filename, out float[] L, out float[] R)
        {
            L = R = null;
            
                using (FileStream fs = File.Open(filename, FileMode.Open))      
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    // chunk 0
                    this.chunkID = reader.ReadChars(4);
                    this.fileSize = reader.ReadInt32();
                    this.riffType = reader.ReadChars(4);
                    // chunk 1
                    this.fmtID = reader.ReadChars(4);
                    this.fmtSize = reader.ReadInt32(); // bytes for this chunk
                    this.fmtCode = reader.ReadInt16();
                    this.channels = reader.ReadInt16();
                    this.sampleRate = reader.ReadInt32();
                    this.byteRate = reader.ReadInt32();
                    this.fmtBlockAlign = reader.ReadInt16();
                    this.bitDepth = reader.ReadInt16();
                    if (fmtSize == 18)
                    {
                        // Read any extra values
                        this.fmtExtraSize = reader.ReadInt16();
                        reader.ReadBytes(this.fmtExtraSize);
                    }
                    // chunk 2
                    this.dataID = reader.ReadChars(4);
                    this.bytes = reader.ReadInt32();
                    // DATA!
                    this.byteArray = reader.ReadBytes(bytes);

                    this.bytesForSamp = this.bitDepth / 8;
                    this.samps = this.bytes / this.bytesForSamp;
                }
                    
                float[] asFloat = null;
                switch (this.bitDepth)
                {
                    case 64:
                        double[] asDouble = new double[this.samps];
                        Buffer.BlockCopy(this.byteArray, 0, asDouble, 0, this.bytes);
                        asFloat = Array.ConvertAll(asDouble, e => (float)e);
                        break;
                    case 32:
                        asFloat = new float[this.samps];
                        Buffer.BlockCopy(this.byteArray, 0, asFloat, 0, this.bytes);
                        break;
                    case 16:
                        Int16[] asInt16 = new Int16[this.samps];
                        Buffer.BlockCopy(this.byteArray, 0, asInt16, 0, this.bytes);
                        asFloat = Array.ConvertAll(asInt16, e => e / (float)Int16.MaxValue);
                        break;
                    default:
                        return false;
                }

                switch (this.channels)
                {
                    case 1:
                        L = asFloat;
                        R = null;
                        return true;
                    case 2:
                        L = new float[this.samps / 2];
                        R = new float[this.samps / 2];
                        for (int i = 0, s = 0; i < this.samps / 2; i++)
                        {
                            L[i] = asFloat[s++];
                            R[i] = asFloat[s++];
                        }
                        return true;
                    default:
                        return false;
                }
            return false;
        }
        public bool writeWav(string filename, float[] L, float[] R)
        {
            float[] asFloat = null;
            switch (this.channels)
            {
                case 1:
                    this.samps = L.Length;
                    asFloat = new float[this.samps];
                    for (int i = 0, s = 0; i < this.samps; i++)
                        asFloat[s++] = L[i];
                    break;
                case 2:
                    this.samps = 2 * L.Length;
                    asFloat = new float[this.samps];
                    for (int i = 0, s = 0; i < this.samps / 2; i++)
                    {
                        asFloat[s++] = L[i];
                        asFloat[s++] = R[i];
                    }
                    break;
                default:
                    return false;
            }
            switch (this.bitDepth)
            {
                case 64:
                    this.bytes = this.samps * this.bytesForSamp;//кол-во байт
                    double[] asDouble = new double[this.samps];
                    asDouble = Array.ConvertAll(asFloat, e => (double)e);
                    Buffer.BlockCopy(asDouble, 0, this.byteArray, 0, this.bytes);
                    break;
                case 32:
                    this.bytes = this.samps * this.bytesForSamp;//кол-во байт
                    Buffer.BlockCopy(asFloat, 0, this.byteArray, 0, this.bytes);
                    break;
                case 16:
                    this.bytes = this.samps * this.bytesForSamp;//кол-во байт
                    Int16[] asInt16 = new Int16[this.samps];
                    asInt16 = asFloat.Select(e => Convert.ToInt16(Math.Round(e * Int16.MaxValue))).ToArray();
                    Buffer.BlockCopy(asInt16, 0, this.byteArray, 0, this.bytes);
                    break;
                default:
                    return false;
            }
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // chunk 0
                writer.Write(this.chunkID);
                writer.Write(this.fileSize);
                writer.Write(this.riffType);
                // chunk 1
                writer.Write(this.fmtID);
                writer.Write(this.fmtSize); // bytes for this chunk
                writer.Write(this.fmtCode);
                writer.Write(this.channels);
                writer.Write(this.sampleRate);
                writer.Write(this.byteRate);
                writer.Write(this.fmtBlockAlign);
                writer.Write(this.bitDepth);
                if (this.fmtSize == 18)
                {
                    // Write any extra values
                    writer.Write(this.fmtExtraSize);
                }
                // chunk 2
                writer.Write(this.dataID);
                writer.Write(this.bytes);
                // DATA!
                for (int i = 0; i < this.byteArray.Length; i++)
                {
                    writer.Write(this.byteArray[i]);
                }
            }
            return true;
        }
    }
}
