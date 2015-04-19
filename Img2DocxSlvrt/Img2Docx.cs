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
using System.IO;
using System.Windows.Resources;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Checksums;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Browser;
using System.Linq;

namespace Img2DocxSlvrt
{
    public class Img2Docx
    {
        private Dictionary<String,FileEntry> _entries = new Dictionary<String, FileEntry>();
        private List<Item> _items = new List<Item>();
        private Dictionary<String, String> _content_types = new Dictionary<String, String>();

        private XNamespace _w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        public Img2Docx()
        {
            _content_types.Add("jpg","images/jpeg");
            _content_types.Add("tif","image/tiff");
            _content_types.Add("png","image/png");
        }

        public void AddImage(String filename,  byte[] buffer)
        {
            filename = filename.Replace(".bmp", ".png");
            ImageItem ii = new ImageItem(filename, buffer);
            _items.Add(ii);
        }

        public void Reset()
        {
            _items.Clear();
           // _entries.Clear();
        }

        public void Save(Stream strm)
        {
            try
            {
                // Extract template
                Extract();

                // Copy images
                CopyImages();

                // Make XML changes
                MakeXmlChanges();

                // Compress and write
                CompressAndWrite(strm);
            }
            catch (Exception ex)
            {
                HtmlPage.Window.Alert(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void Extract()
        {
            // Get template
            StreamResourceInfo sr = Application.GetResourceStream(
                new Uri("Img2DocxSlvrt;component/template.docx", UriKind.Relative));

             // Extract
            _entries.Clear();
            using (ZipInputStream zis = new ZipInputStream(sr.Stream))
            {
                //try
                //{
                    ZipEntry ze = zis.GetNextEntry();
                    while (ze != null)
                    {
                        _entries.Add(ze.Name, new FileEntry(zis, ze.Name));
                        ze = zis.GetNextEntry();
                    }
                //}
                //catch (Exception ex)
                //{
                //    int t = 4;
                //    t = 4;
                //}
                zis.Close();
             }
        }

        private void CompressAndWrite(Stream strm)
        {
            using (ZipOutputStream zos = new ZipOutputStream(strm))
            {
                zos.SetLevel(1);
                foreach (KeyValuePair<String, FileEntry> pair in _entries)
                        pair.Value.Write(zos);
                zos.Finish();
                zos.Close();
            }
            //byte[] bytes = new byte[sr.Stream.Length];
            //sr.Stream.Read(bytes, 0, (int)sr.Stream.Length);
            //strm.Write(bytes, 0, (int)sr.Stream.Length);
        }

        private void CopyImages()
        {
            int n = 1;
            foreach (Item item in _items)
            {
                if (item is ImageItem)
                {
                    ImageItem ii = item as ImageItem;
                    String ext = System.IO.Path.GetExtension(ii.Filename);
                    String target = "image" + n.ToString() + ext;
                    ii.Target = target;

                    FileEntry fe = new FileEntry(ii.Buffer, "word/media/"+ii.Target);
                    _entries.Add(ii.Filename, fe);
                    n++;
                }
            }
        }

        private void MakeXmlChanges()
        {
            AddContentType();

            MakeReferences();

            InsertItems();

            InsertChars();

            InsertTexts();

            ApplyProperties();

            RemoveGarbage();

        }

        private void AddContentType()
        {
            FileEntry fe = _entries["[Content_Types].xml"];

            fe.Ms.Position = 0;
            XDocument doc;
            using (XmlReader xr = XmlReader.Create(fe.Ms))
            {
                doc = XDocument.Load(xr);
                XNamespace ns = doc.Root.GetDefaultNamespace();
                Dictionary<String, bool> unique_exts = new Dictionary<string, bool>();
                foreach (Item item in _items)
                {
                    if (item is ImageItem)
                    {
                        ImageItem ii = item as ImageItem;
                        String ext = System.IO.Path.GetExtension(ii.Filename).Remove(0, 1);
                        if (!unique_exts.ContainsKey(ext))
                        {
                            doc.Root.AddFirst(new XElement(ns + "Default",
                                new XAttribute("Extension", ext), new XAttribute("ContentType", _content_types[ext])));
                            unique_exts[ext] = true;
                        }
                    }
                }
            }
            fe.Ms.Position = 0;
            using (XmlWriter xw = XmlWriter.Create(fe.Ms))
                doc.Save(xw);
        }   

        private void MakeReferences()
        {
            FileEntry fe = _entries["word/_rels/document.xml.rels"];
            fe.Ms.Position = 0;
            XDocument doc;
            using (XmlReader xr = XmlReader.Create(fe.Ms))
            {
                doc = XDocument.Load(xr);
                XNamespace ns = doc.Root.GetDefaultNamespace();
       
                foreach (Item item in _items)
                {
                    if (item is ImageItem)
                    {
                        ImageItem ii = item as ImageItem;
                        ii.ID = "rId" + (doc.Root.Elements().Count()+1).ToString();
                        doc.Root.AddFirst(new XElement(ns + "Relationship",
                            new XAttribute("Id", ii.ID),
                            new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image"),
                            new XAttribute("Target", "media/"+ii.Target)
                            ));
                    }
                }
            }
            fe.Ms.Position = 0;
            using (XmlWriter xw = XmlWriter.Create(fe.Ms))
                doc.Save(xw);
        }

        private void InsertItems()
        {
            FileEntry fe = _entries["word/document.xml"];
            fe.Ms.Position = 0;
            XDocument doc;
            using (XmlReader xr = XmlReader.Create(fe.Ms))
            {
                doc = XDocument.Load(xr);
                XNamespace ns = doc.Root.GetDefaultNamespace();
                foreach (XElement el in doc.Root.Descendants(_w + "body"))
                {
                    XNode body_node = el.FirstNode;
                    XNode curr_node = el.FirstNode;
                    int inum = 1;
                    for( int i = 0; i < _items.Count; i++)
                    {
                        XElement p = new XElement(_w + "p", 
                            new XAttribute(_w + "rsidR", "00903E01"), 
                            new XAttribute(_w + "rsidRDefault", "00903E01"));
                        body_node.AddAfterSelf(p);
                        if (_items[i] is ImageItem)
                        {
                            ImageItem ii = _items[i] as ImageItem;
                            InsertImage(p, ii, inum);
                            inum++;
                            curr_node = p;

                            if (i != _items.Count - 1)
                            {
                                XElement p1 = new XElement(_w + "p",
                                    new XAttribute(_w + "rsidR", "00903E01"),
                                    new XAttribute(_w + "rsidRDefault", "00903E01"),
                                    new XElement(_w + "r",
                                           new XElement(_w + "rPr", new XElement(_w + "lang", new XAttribute(_w + "val","en-US"))),
                                           new XElement(_w + "br",  new XAttribute(_w + "type", "page"))                                        
                                        ));
                                body_node.AddAfterSelf(p1);
                                curr_node = p1;
                            }

                        }                       
                        p.AddFirst(new XElement(_w + "pPr", new XElement(_w + "rPr", new XElement(_w + "lang", new XAttribute(_w + "val","en-US")))));
                   }
                }
            }
            fe.Ms.Position = 0;
            using (XmlWriter xw = XmlWriter.Create(fe.Ms))
                doc.Save(xw);
        }

        private void InsertImage(XElement elem, ImageItem ii, int n)
        {
        }

        private void InsertChars()
        {
        }

        private void InsertTexts()
        {
        }

        private void ApplyProperties()
        {
        }

        private void RemoveGarbage()
        {
        }
    }
}
