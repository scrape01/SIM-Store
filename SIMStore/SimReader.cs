using System;
using System.Collections.Generic;
using System.Text;

namespace SIMStore
{
    class SimReader
    {
        private IntPtr nContext = IntPtr.Zero;          // Card reader context handle - DWORD
        private IntPtr nCard = IntPtr.Zero;             // Connection handle - DWORD
        private IntPtr nActiveProtocol = new IntPtr(0); // T0/T1
        private int nNotUsed1 = 0;
        private int nNotUsed2 = 0;
        private IntPtr readerNameLen = new IntPtr(0);   // Selected reader name len		
        private int cardProtocol = 0;
        private byte[] atrValue;                        // ATR content
        private IntPtr atrLen = new IntPtr(33);         // ATR length
        private int ret = 0;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int MaxRecordLength;
        private int MaxAtrFileLength;
        private int MaxRecordCount;

        public SimReader()
        {
            if (CreateContext() != "")
            {
                // Error detected
                return;
            }
            UpdateListReaders();
        }

        private string CreateContext()
        {
            // Create PCSC context
            int ret = WinSCard.SCardEstablishContext(WinSCard.SCARD_SCOPE_SYSTEM, nNotUsed1, nNotUsed2, ref nContext);

            if (ret != 0)
            {
                // Error detected
                nContext = IntPtr.Zero;
                log.Error("CreateContext " + WinSCard.ParseError(ret));
                return WinSCard.ParseError(ret);
            }

            log.Debug("CreateContext " + WinSCard.ParseError(ret));
            return "";
        }

        /// <summary>
        /// Updates list of readers
        /// </summary>
        private void UpdateListReaders()
        {
            byte[] readersBuf = new byte[0];
            IntPtr readersBufLen = IntPtr.Zero;
            List<byte> lreaders = new List<byte>();
            List<string> lstreaders = new List<string>();

            //First time to retrieve the len of buffer for readers name
            ret = WinSCard.SCardListReaders(nContext, null, readersBuf, out readersBufLen);

            // Redim buffer
            readersBuf = new byte[readersBufLen.ToInt32()];

            // Second time to retrieve readers name
            ret = WinSCard.SCardListReaders(nContext, null, readersBuf, out readersBufLen);

            if (ret != 0)
            {
                // Error detected
                log.Error("PcscReader::UpdateListReaders: SCardListReaders " + WinSCard.ParseError(ret));
                return;
            }

            if (readersBuf.Length < 5)
            {
                // No readers founded
                return;
            }

            // Loop to detect readers
            for (int j = 0; j < readersBuf.Length; j++)
            {
                // check for first null byte
                if (readersBuf[j] != 0x00)
                {
                    // Add byte to byte list
                    lreaders.Add(readersBuf[j]);
                }
                else
                {
                    //check for second null byte (end of list)
                    if (readersBuf[j - 1] == 0x00)
                    {
                        break;
                    }

                    // Update readers list
                    lstreaders.Add(Encoding.ASCII.GetString(lreaders.ToArray()));
                    lreaders = new List<byte>();
                }
            }
            // update public list
            Readers = lstreaders;
        }

        /// <summary>
        /// PCSC readers list
        /// </summary>
        public List<string> Readers { get; private set; } = new List<string>();

        public int GetMaxFileLength (string fileName)
        {
            return MaxAtrFileLength - sizeof(int) - sizeof(int) - (fileName.Length * 2);
        }

        public string SelectedReader { get; set; }



        /// <summary>
        /// Disconnect smartcard from reader
        /// </summary>
        public void CloseConnection()
        {
            // check for card context
            if (nCard.ToInt64() != 0)
            {
                // disconnect
                ret = WinSCard.SCardDisconnect(nCard, (uint)WinSCard.SCARD_DISPOSITION.SCARD_UNPOWER_CARD);
                log.Debug("CloseConnection " + ret.ToString());
                nCard = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Write file
        /// File name size (4 bytes)
        /// File name (MaxFileNameLength bytes)
        /// File size (4 bytes)
        /// File content
        /// </summary>
        internal bool WriteFile(string fileName, byte[] fileBytes)
        {
            string record = string.Empty;
            byte[] fileNameSizeBytes = BitConverter.GetBytes(fileName.Length);
            byte[] fileSizeBytes = BitConverter.GetBytes(fileBytes.Length);
            record = Utility.GetHexFromBytes(fileNameSizeBytes) +
                Utility.HexFromAscii(fileName) + 
                Utility.GetHexFromBytes(fileSizeBytes) + 
                Utility.GetHexFromBytes(fileBytes);
            List<string> records = new List<string>(Utility.ChunkString(record, MaxRecordLength * 2));
            int i = 1;
            string expRecord = "9000";
            foreach (string rec in records)
            {
                string simCommand = "A0DC" + i.ToString("X2") + "04" + MaxRecordLength.ToString("X2");
                simCommand += rec.PadRight(MaxRecordLength * 2, 'F');
                string simExpResponse = expRecord;
                string simResponse = "";
                bool simRespOk = false;
                if (SendReceiveAdv(simCommand, ref simResponse, simExpResponse, ref simRespOk))
                {
                    log.Debug("Successfully wrote SIM record at " + i);
                } else
                {
                    log.Error("Error writing line " + i + " to SIM");
                    break;
                }
                i++;
                System.Threading.Thread.Sleep(30);
            }
            return true;
        }

        /// <summary>
        /// Read file
        /// File name (MaxFileNameLength bytes)
        /// File size (4 bytes)
        /// File content
        /// </summary>
        internal bool ReadFile(ref string fileName, ref byte[] fileBytes)
        {
            string expRecord = new string('?', MaxRecordLength * 2) + "9000";
            StringBuilder fileAsString = new StringBuilder();
            bool ret = true;
            for (int i = 1; i <= MaxRecordCount; i++)
            {
                string simCommand = "A0B2" + i.ToString("X2") + "04" + MaxRecordLength.ToString("X2");
                string simExpResponse = expRecord;
                string simResponse = "";
                bool simRespOk = false;
                if (SendReceiveAdv(simCommand, ref simResponse, simExpResponse, ref simRespOk))
                {
                    if (simRespOk)
                    {
                        log.Debug("Successfully read SIM record at " + i);
                        fileAsString.Append(simResponse.Substring(0, simResponse.Length - 4));
                    } else
                    {
                        log.Error("Error reading line " + i + " from SIM. simRespOk = false");
                        ret = false;
                        break;
                    }
                } else
                {
                    log.Error("Error reading line " + i + " from SIM");
                    ret = false;
                    break;
                }
                System.Threading.Thread.Sleep(30);
            }
            if (ret == true && fileAsString.Length > 0)
            {
                string fileString = fileAsString.ToString();
                int fileNameSize = Utility.GetIntFromHex(fileString.Substring(0, sizeof(int) * 2));
                string fileNameRaw = fileAsString.ToString().Substring(sizeof(int) * 2, fileNameSize * 2);
                fileName = Utility.StringFromHex(fileNameRaw);
                int fileSize = Utility.GetIntFromHex(fileString.Substring((sizeof(int) * 2) + (fileNameSize * 2), (sizeof(int) * 2)));
                fileBytes = Utility.GetBytesFromHex(fileString.Substring((sizeof(int) * 2) + (fileNameSize * 2) + (sizeof(int) * 2), fileSize * 2));
            }

            return ret;
        }

        /// <summary>
        /// Release PCSC context
        /// </summary>
        private void ReleaseContext()
        {
            if (nContext.ToInt64() != 0)
            {
                // Release PCSC context
                ret = WinSCard.SCardReleaseContext(nContext);

                log.Debug("ReleaseContext " + WinSCard.ParseError(ret));
                nContext = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Get ATR and smartcard status
        /// </summary>
        /// <returns>
        /// Empty or error message
        /// </returns>
        public bool ReaderStatus() //ref string response)
        {
            // set empty byte array
            atrValue = new byte[33];

            byte[] retRName = new byte[64];
            readerNameLen = new IntPtr(64);
            atrLen = new IntPtr(33);
            cardProtocol = 1;
            uint readerState = 0;

            // firts time to set sizes
            ret = WinSCard.SCardStatus(nCard, retRName, ref readerNameLen, out readerState,
                              out cardProtocol, atrValue, ref atrLen);

            if (ret != 0)
            {
                // reset byte array to returned length and retry			
                atrValue = new byte[atrLen.ToInt32()];
                retRName = new byte[readerNameLen.ToInt32()];

                ret = WinSCard.SCardStatus(nCard, retRName, ref readerNameLen, out readerState,
                              out cardProtocol, atrValue, ref atrLen);
            }


            if (ret != 0)
            {
                // Error detected
                log.Error("ReaderStatus " + WinSCard.ParseError(ret));
                return false;
            }

            // Extract ATR value
            var response = Utility.GetHexFromBytes(atrValue, 0, atrLen.ToInt32());
            log.Debug("ATR value: " + response);

            return true;
        }

        /// <summary>
        /// Power On smartcard
        /// </summary>
        /// <returns>
        /// Empty or error message
        /// </returns>
        public bool AnswerToReset() //ref string response)
        {
            string retContext = "";

            // check for selected reader
            if (SelectedReader == "")
            {
                log.Error("Reader not set");
                return false;
            }

            // close connection and context
            CloseConnection();
            System.Threading.Thread.Sleep(20);
            ReleaseContext();

            // delay before power on
            System.Threading.Thread.Sleep(200);

            // Try to create new context
            retContext = CreateContext();
            if (retContext != "")
            {
                // error detected
                return false;
            }

            System.Threading.Thread.Sleep(20);

            // Connect to smartcard
            int ret = WinSCard.SCardConnect(nContext, SelectedReader,
                                   (uint)WinSCard.SCARD_SHARE.SCARD_SHARE_SHARED,
                                   (uint)WinSCard.SCARD_PROTOCOL.SCARD_PROTOCOL_T0 |
                                   (uint)WinSCard.SCARD_PROTOCOL.SCARD_PROTOCOL_T1,
                               ref nCard, ref nActiveProtocol);

            if (ret != 0)
            {
                // Error detected
                log.Error("AnswerToReset " + WinSCard.ParseError(ret));
                return false;
            }


            // Get ATR
            bool retStatus = ReaderStatus();
            if (!retStatus)
            {
                log.Error("AnswerToReset - Error getting ATR");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Select ADN file on sim and extract main info
        /// </summary>
        public bool ReadADN()
        {
            // Select 7F10 (DF TELECOM)
            string simCommand = "A0A40000027F10";
            string simExpResponse = "9F??";
            string simResponse = string.Empty;
            bool simRespOk = false;
            bool ret = SendReceiveAdv(simCommand, ref simResponse, simExpResponse, ref simRespOk);
            log.Debug("GlobalObjUI::ReadADN: SELECT DF TELECOM " + simResponse);

            if (!ret)
            {
 
                return ret;
            }

            if (!simRespOk)
            {
                log.Error("WRONG RESPONSE [" + simExpResponse + "] " + "[" + simResponse + "]");
                return false;
            }


            // Select 6F3A (ADN)
            simCommand = "A0A40000026F3A";
            simExpResponse = "9F??";
            ret = SendReceiveAdv(simCommand, ref simResponse, simExpResponse, ref simRespOk);
            Console.WriteLine("GlobalObjUI::ReadADN: SELECT ADN " + simResponse);

            if (!ret)
            {
                return false;
            }

            if (!simRespOk)
            {
                log.Error("WRONG RESPONSE [" + simExpResponse + "] " + "[" + simResponse + "]");
                return false;
            }



            // Get Response 6F3A (ADN)
            simCommand = "A0C00000" + simResponse.Substring(2, 2);
            simExpResponse = new string('?', Convert.ToInt32(simResponse.Substring(2, 2), 16) * 2) + "9000";
            ret = SendReceiveAdv(simCommand, ref simResponse, simExpResponse, ref simRespOk);
            Console.WriteLine("GlobalObjUI::ReadADN: GET RESPONSE " + simResponse);

            if (!ret)
            {
                return false;
            }

            if (!simRespOk)
            {
                log.Error("WRONG RESPONSE [" + simExpResponse + "] " + "[" + simResponse + "]");
                return false;
            }

            // Update ADN values
            MaxRecordLength = Convert.ToInt32(simResponse.Substring(28, 2), 16);
            MaxAtrFileLength = Convert.ToInt32(simResponse.Substring(4, 4), 16);
            MaxRecordCount = MaxAtrFileLength / MaxRecordLength;
            log.Debug("GlobalObjUI::ReadADN: Record len .... : " + MaxRecordLength.ToString());
            log.Debug("GlobalObjUI::ReadADN: Record count .. : " + MaxRecordCount.ToString());
            return true;
        }



        /// <summary>
        /// Exchange data with smartcard and check response with expected data, 
        /// you can use '?' digit to skip check in a specific position.
        /// </summary>
        private bool SendReceiveAdv(string command,
                                        ref string response,
                                            string expResponse,
                                        ref bool isVerified)
        {
            isVerified = false;
            response = "";

            // exchange data
            bool ret = SendReceive(command, ref response);

            if (!ret)
            {
                // error detected
                return ret;
            }

            if (response.Length != expResponse.Length)
            {
                // two length are differents
                log.Error("Response lenght does not match expected");
                return false;
            }

            // loop for each digits
            for (int p = 0; p < response.Length; p++)
            {
                if ((expResponse.Substring(p, 1) != "?") &&
                    (expResponse.Substring(p, 1) != response.Substring(p, 1)))
                {
                    // data returned is different from expected
                    log.Error("Response data does not match expected");
                    return false;
                }
            }
            isVerified = true;
            return true;
        }

        /// <summary>
        /// Exchange data with smartcard
        /// </summary>
        /// <returns>
        /// Empty or error message
        /// </returns>
        public bool SendReceive(string command, ref string response)
        {
            // check for selected reader
            if (SelectedReader == "")
            {
                log.Error("Card reader not selected");
                return false;
            }
            IntPtr retCommandLen = new IntPtr(261);

            // remove unused digits
            command = command.Trim().Replace("0x", "").Replace(" ", "");

            // Set input command byte array
            byte[] inCommandByte = Utility.GetBytesFromHex(command);
            byte[] outCommandByte = new byte[261];

            // Prepare structures
            WinSCard.SCARD_IO_REQUEST pioSend = new WinSCard.SCARD_IO_REQUEST();
            WinSCard.SCARD_IO_REQUEST pioRecv = new WinSCard.SCARD_IO_REQUEST();
            pioSend.dwProtocol = (uint)cardProtocol;
            pioSend.cbPciLength = (uint)8;
            pioRecv.dwProtocol = (uint)cardProtocol;
            pioRecv.cbPciLength = 0;

            // exchange data with smartcard
            int ret = WinSCard.SCardTransmit(nCard,
                                ref pioSend,
                                    inCommandByte,
                                    inCommandByte.Length,
                                    IntPtr.Zero,
                                    outCommandByte,
                                out retCommandLen);
            if (ret != 0)
            {
                // Error detected
                log.Error("PcScReader.IReader::SendReceive: SCardTransmit " + WinSCard.ParseError(ret));
                return false;
            }
            // Extract response
            response = Utility.GetHexFromBytes(outCommandByte, 0, retCommandLen.ToInt32()).Trim();
            return true;
        }
    }
}