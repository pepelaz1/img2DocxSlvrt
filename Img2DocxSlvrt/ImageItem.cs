using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.IO;

namespace Img2DocxSlvrt
{
    public class ImageItem : Item
    {
        private String _id;
        public String ID
        {
            get { return _id; }
            set { _id = value; }
        }

        private byte [] _buffer;
        public byte []Buffer
        {
            get { return _buffer; }
        }

        private String _target;
        public String Target
        {
            get { return _target; }
            set { _target = value; }
        }


        public ImageItem(String filename, byte[] buffer)
        {
            Filename = filename;
            _buffer = new byte[buffer.Length];
            buffer.CopyTo(_buffer, 0);
        }
    }
}
