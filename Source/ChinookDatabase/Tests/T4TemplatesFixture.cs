﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using NUnit.Framework;

namespace ChinookDatabase.Tests
{
    [TestFixture]
    public class T4TemplatesFixture
    {
        [Test]
        public void ReadiTunesLibrary2()
        {
            FileInfo thisFile = new FileInfo(@"C:\Code\ChinookDatabase\Trunk\Source\ChinookDatabase\Tests\T4TemplatesFixture.cs");
            string filename = thisFile.DirectoryName + @"\..\SampleData\iTunes Music Library.xml";
            Assert.That(File.Exists(filename));

            DataSet ds = new DataSet();
            ds.ReadXmlSchema(thisFile.DirectoryName + @"\..\Schema\ChinookDataset.xsd");
            ds.ReadXml(thisFile.DirectoryName + @"\..\SampleData\ManualData.xml");

            int artistId = 0;
            int? albumId = null;
            string name = "";
            int time = 0;
            int mediaTypeId = 0;
            int size = 0;
            bool skipTrack = false;

            FileStream file = File.OpenRead(filename);
            StreamReader reader = new StreamReader(file);

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                if (line == "\t\t<dict>")
                {
                    artistId = 0;
                    albumId = null;
                    name = "";
                    time = 0;
                    mediaTypeId = 0;
                    size = 0;
                    skipTrack = false;
                    continue;
                }

                if (line == "\t\t</dict>" && !skipTrack && albumId!=null)
                {
                    ds.Tables["Track"].Rows.Add(ds.Tables["Track"].Rows.Count + 1, name, albumId, mediaTypeId, time, size, 0.99);
                    continue;
                }

                if (line == "			<key>Podcast</key><true/>")
                {
                    skipTrack = true;
                    continue;
                }

                Match m = Regex.Match(line, "\t\t\t<key>Artist</key><string>(.*)</string>");
                if (m.Success)
                {
                    string artistName = m.Groups[1].ToString();

                    DataRow[] results = ds.Tables["Artist"].Select("Name = '" + artistName.Replace("'", "''") + "'");

                    if (results == null || results.Length == 0)
                    {
                        artistId = ds.Tables["Artist"].Rows.Count + 1;
                        ds.Tables["Artist"].Rows.Add(artistId, artistName);
                    }
                    else
                    {
                        artistId = (int) results[0]["ArtistId"];  
                    }
                    continue;
                }

                m = Regex.Match(line, "\t\t\t<key>Album</key><string>(.*)</string>");
                if (m.Success)
                {
                    string albumTitle = m.Groups[1].ToString();

                    DataRow[] results = ds.Tables["Album"].Select("Title = '" + albumTitle.Replace("'", "''") + "'");

                    if (results == null || results.Length == 0)
                    {
                        albumId = ds.Tables["Album"].Rows.Count + 1;
                        ds.Tables["Album"].Rows.Add(albumId, albumTitle, artistId);
                    }
                    else
                    {
                        albumId = (int) results[0]["AlbumId"];  
                    }
                    continue;
                }

                m = Regex.Match(line, "\t\t\t<key>Kind</key><string>(.*)</string>");
                if (m.Success)
                {
                    string mediaType = m.Groups[1].ToString();

                    DataRow[] results = ds.Tables["MediaType"].Select("Name = '" + mediaType.Replace("'", "''") + "'");

                    if (results == null || results.Length == 0)
                    {
                        mediaTypeId = ds.Tables["MediaType"].Rows.Count + 1;
                        ds.Tables["MediaType"].Rows.Add(mediaTypeId, mediaType);
                    }
                    else
                    {
                        mediaTypeId = (int)results[0]["MediaTypeId"];
                    }
                    continue;
                }

                m = Regex.Match(line, "\t\t\t<key>Name</key><string>(.*)</string>");
                if (m.Success)
                {
                    name = m.Groups[1].ToString();
                    continue;
                }

                m = Regex.Match(line, "\t\t\t<key>Size</key><integer>(\\d*)</integer>");
                if (m.Success)
                {
                    int.TryParse(m.Groups[1].ToString(), out size);
                    continue;
                }

                m = Regex.Match(line, "\t\t\t<key>Total Time</key><integer>(\\d*)</integer>");
                if (m.Success)
                {
                    if (!int.TryParse(m.Groups[1].ToString(), out time) || time==0)
                        skipTrack = true;
                }
            }

            //ds.WriteXml(@"C:\Code\ChinookDatabase\Trunk\Source\ChinookDatabase\SampleData\ChinookData.xml");

            MemoryStream memstream = new MemoryStream();
            ds.WriteXml(memstream);
            memstream.Position = 0;
            StreamReader sr = new StreamReader(memstream);
            Trace.WriteLine(sr.ReadToEnd());
        }

    }
}