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
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using System.Windows.Media.Imaging;

namespace Img2DocxSlvrt
{
    public class FileEntry
    {
        private String _filename;
        public String Filename
        {
            get { return _filename; }
        }
        private MemoryStream _ms = new MemoryStream();
        public MemoryStream Ms
        {
            get { return _ms; }
        }
       /* public FileEntry(ZipInputStream zis, String filename)
        {
            _filename = filename;
            byte[] buffer = new byte[2048];
            int count;
            while ((count = zis.Read(buffer, 0, buffer.Length)) != 0)
                _ms.Write(buffer, 0, count);
        }*/

        public FileEntry(Stream strm, String filename)
        {
            _filename = filename;
            byte[] buffer = new byte[2048];
            int count;
            while ((count = strm.Read(buffer, 0, buffer.Length)) != 0)
                _ms.Write(buffer, 0, count);
         }

         public FileEntry(byte[] buffer, String filename)
         {
             _filename = filename;
             _ms.Write(buffer, 0, buffer.Length);
         }
        
        public void Write(ZipOutputStream zos)
        {
            ZipEntry ze = new ZipEntry(_filename);
            ze.DateTime = DateTime.Now;
            ze.Size = _ms.Length;

            byte[] buffer = new byte[_ms.Length];
            _ms.Position = 0;
            _ms.Read(buffer, 0, buffer.Length);

            zos.PutNextEntry(ze);
            zos.Write(buffer, 0, buffer.Length);
       }
    }
}
