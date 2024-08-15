﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAXLib;

namespace Xv2CoreLib.OCO
{
    public class Parser
    {
        private string saveLocation;
        private byte[] rawBytes;
        public OCO_File ocoFile { get; private set; } = new OCO_File();

        public Parser(string path, bool _writeXml)
        {
            saveLocation = path;
            rawBytes = File.ReadAllBytes(path);

            Parse();

            if (_writeXml)
            {
                YAXSerializer serializer = new YAXSerializer(typeof(OCO_File));
                serializer.SerializeToFile(ocoFile, saveLocation + ".xml");
            }
        }

        public Parser(byte[] bytes)
        {
            rawBytes = bytes;

            Parse();
        }

        private void Parse()
        {
            ocoFile.Version = BitConverter.ToUInt16(rawBytes, 6);
            uint count = BitConverter.ToUInt32(rawBytes, 8);
            int offset = 16;

            if (count > 0)
            {
                ocoFile.Partners = new List<OCO_Partner>();

                for(int i = 0; i < count; i++)
                {
                    int subEntryCount = BitConverter.ToInt32(rawBytes, offset);
                    int subDataOffset = BitConverter.ToInt32(rawBytes, offset + 4) + (GetSubEntryDataSize() * BitConverter.ToInt32(rawBytes, offset + 8)) + 16;
                    ocoFile.Partners.Add(new OCO_Partner() { PartnerID = BitConverter.ToInt32(rawBytes, offset + 12) });
                    
                    if(subEntryCount > 0)
                    {
                        ocoFile.Partners[i].SubEntries = new List<OCO_Costume>();
                    }

                    for (int a = 0; a < subEntryCount; a++)
                    {
                        switch (ocoFile.Version)
                        {
                            case 16:
                                ocoFile.Partners[i].SubEntries.Add(new OCO_Costume()
                                {
                                    I_04 = BitConverter.ToInt32(rawBytes, subDataOffset + 4),
                                    I_08 = BitConverter.ToInt32(rawBytes, subDataOffset + 8),
                                    I_12 = BitConverter.ToInt32(rawBytes, subDataOffset + 12),
                                    I_16 = BitConverter.ToInt32(rawBytes, subDataOffset + 16)
                                });

                                subDataOffset += 20;
                                break;
                            case 20:
                                ocoFile.Partners[i].SubEntries.Add(new OCO_Costume()
                                {
                                    I_04 = BitConverter.ToInt32(rawBytes, subDataOffset + 4),
                                    I_08 = BitConverter.ToInt32(rawBytes, subDataOffset + 8),
                                    I_12 = BitConverter.ToInt32(rawBytes, subDataOffset + 12),
                                    NEW_I_16 = BitConverter.ToInt32(rawBytes, subDataOffset + 16),
                                    I_16 = BitConverter.ToInt32(rawBytes, subDataOffset + 20)
                                });

                                subDataOffset += 24;
                                break;
                            default:
                                throw new Exception("Unknown OCO version.");
                        }
                    }
                    offset += 16;
                }
                
            }
        }
        private int GetSubEntryDataSize()
        {
            switch (ocoFile.Version)
            {
                case 16:
                    return 20;
                case 20:
                    return 24;
                default:
                    throw new Exception("Unknown OCP version.");
            }
        }
    }
}
