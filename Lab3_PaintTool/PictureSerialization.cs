using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Serialization;

namespace Lab3_PaintTool
{
    [Serializable]
    public class PictureSerialization
    {
        [XmlIgnore]
        public Bitmap PaintImage { get; set; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)] [XmlElement("SerializePaint")]
        public byte[] IconSerialized;

        public void Serialize()
        {
            using (var ms = new MemoryStream())
            {
                PaintImage.Save(ms, ImageFormat.Bmp);
                IconSerialized = ms.ToArray();
            }
        }
        
        public void Deserialize()
        {
            using (var ms = new MemoryStream(IconSerialized))
                PaintImage = new Bitmap(ms);
        }
    }
}