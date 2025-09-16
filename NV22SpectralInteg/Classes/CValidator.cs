
using ITLlib;
using NV22SpectralInteg.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg.Classes
{
    public class CValidator
    {
        // ssp library variables
        SSPComms m_eSSP;
        SSP_COMMAND m_cmd;
        SSP_KEYS keys;
        SSP_FULL_KEY sspKey;
        SSP_COMMAND_INFO info;

        // variable declarations

        // The comms window class, used to log everything sent to the validator visually and to file
        //CCommsWindow m_Comms;

        // The protocol version this validator is using, set in setup request
        int m_ProtocolVersion;

        // A variable to hold the type of validator, this variable is initialised using the setup request command
        char m_UnitType;

        // Two variables to hold the number of notes accepted by the validator and the value of those
        // notes when added up
        int m_NumStackedNotes;

        // Variable to hold the number of channels in the validator dataset
        int m_NumberOfChannels;

        // The multiplier by which the channel values are multiplied to give their
        // real penny value. E.g. £5.00 on channel 1, the value would be 5 and the multiplier
        // 100.
        int m_ValueMultiplier;

        //Integer to hold total number of Hold messages to be issued before releasing note from escrow
        int m_HoldNumber;

        //Integer to hold number of hold messages still to be issued
        int m_HoldCount;

        //Bool to hold flag set to true if a note is being held in escrow
        bool m_NoteHeld;

        // A list of dataset data, sorted by value. Holds the info on channel number, value, currency,
        // level and whether it is being recycled.
        List<ChannelData> m_UnitDataList;

        // constructor
        public CValidator()
        {
            m_eSSP = new SSPComms();
            m_cmd = new SSP_COMMAND();
            keys = new SSP_KEYS();
            sspKey = new SSP_FULL_KEY();
            info = new SSP_COMMAND_INFO();

            //m_Comms = new CCommsWindow("NoteValidator");
            m_NumberOfChannels = 0;
            m_ValueMultiplier = 1;
            m_UnitType = (char)0xFF;
            m_UnitDataList = new List<ChannelData>();
            m_HoldCount = 0;
            m_HoldNumber = 0;
        }

        /* Variable Access */

        // access to ssp variables
        // the pointer which gives access to library functions such as open com port, send command etc
        public SSPComms SSPComms
        {
            get { return m_eSSP; }
            set { m_eSSP = value; }
        }

        // a pointer to the command structure, this struct is filled with info and then compiled into
        // a packet by the library and sent to the validator
        public SSP_COMMAND CommandStructure
        {
            get { return m_cmd; }
            set { m_cmd = value; }
        }

        // pointer to an information structure which accompanies the command structure
        public SSP_COMMAND_INFO InfoStructure
        {
            get { return info; }
            set { info = value; }
        }

        // access to the comms log for recording new log messages
        //public CCommsWindow CommsLog
        //{
        //    get { return m_Comms; }
        //    set { m_Comms = value; }
        //}

        // access to the type of unit, this will only be valid after the setup request
        public char UnitType
        {
            get { return m_UnitType; }
        }

        // access to number of channels being used by the validator
        public int NumberOfChannels
        {
            get { return m_NumberOfChannels; }
            set { m_NumberOfChannels = value; }
        }

        // access to number of notes stacked
        public int NumberOfNotesStacked
        {
            get { return m_NumStackedNotes; }
            set { m_NumStackedNotes = value; }
        }

        // access to value multiplier
        public int Multiplier
        {
            get { return m_ValueMultiplier; }
            set { m_ValueMultiplier = value; }
        }
        // acccess to hold number
        public int HoldNumber
        {
            get { return m_HoldNumber; }
            set { m_HoldNumber = value; }

        }
        //Access to flag showing note is held in escrow
        public bool NoteHeld
        {
            get { return m_NoteHeld; }
        }
        // get a channel value
        public int GetChannelValue(int channelNum)
        {
            if (channelNum >= 1 && channelNum <= m_NumberOfChannels)
            {
                foreach (ChannelData d in m_UnitDataList)
                {
                    if (d.Channel == channelNum)
                        return d.Value;
                }
            }
            return -1;
        }

        // get a channel currency
        public string GetChannelCurrency(int channelNum)
        {
            if (channelNum >= 1 && channelNum <= m_NumberOfChannels)
            {
                foreach (ChannelData d in m_UnitDataList)
                {
                    if (d.Channel == channelNum)
                        return new string(d.Currency);
                }
            }
            return "";
        }

        /* Command functions */

        // The enable command allows the validator to receive and act on commands sent to it.
        public void EnableValidator(TextBox log = null)
        {
            Logger.Log("⚡ Attempting to enable validator...");

            m_cmd.CommandData[0] = CCommands.SSP_CMD_ENABLE;
            m_cmd.CommandDataLength = 1;

            if (!SendCommand(log))
            {
                Logger.Log("❌ Failed to send ENABLE command.");
                return;
            }
            if (CheckGenericResponses(log) && log != null)
            {
                Logger.Log("✅ Validator enabled successfully.");
                log.AppendText("Unit enabled\r\n");
            }
            else
            {
                Logger.Log("⚠️ ENABLE command did not return expected response.");
            }
        }


        // Disable command stops the validator from acting on commands.
        public void DisableValidator(TextBox log = null)
        {
            Logger.Log("🛑 Sending DISABLE command...");

            m_cmd.CommandData[0] = CCommands.SSP_CMD_DISABLE;
            m_cmd.CommandDataLength = 1;

            if (!SendCommand(log))
            {
                Logger.Log("❌ Failed to send DISABLE command.");
                return;
            }
            else
            {
                Logger.Log("ℹ️ No ACK received for DISABLE (unit may already be idle).");
            }


            // Don’t require ACK, just log best effort
            Logger.Log("✅ Disable command sent (no ACK expected).");
            log?.AppendText("Unit disabled\r\n");
        }


        // Return Note command returns note held in escrow to bezel. 
        public void ReturnNote(TextBox log = null)
        {
            Logger.Log("↩️ Sending RETURN command (reject banknote)...");

            m_cmd.CommandData[0] = CCommands.SSP_CMD_REJECT_BANKNOTE;
            m_cmd.CommandDataLength = 1;
            if (!SendCommand(log))
            {
                Logger.Log("❌ Failed to send RETURN command.");
                return;
            }

            if (CheckGenericResponses(log))
            {
                log?.AppendText("↩️ Returning note\r\n");
                Logger.Log("✅ Note return initiated.");
                m_HoldCount = 0;
            }
            else
            {
                Logger.Log("⚠️ RETURN command not acknowledged.");
            }
        }

        // The reset command instructs the validator to restart (same effect as switching on and off)
        public void Reset(TextBox log = null)
        {
            Logger.Log("🔄 Sending RESET command to validator...");

            m_cmd.CommandData[0] = CCommands.SSP_CMD_RESET;
            m_cmd.CommandDataLength = 1;
            if (!SendCommand(log))
            {
                Logger.Log("❌ Reset command failed (SendCommand).");
                return;
            }

            if (CheckGenericResponses(log))
            {
                Logger.Log("✅ Validator reset successfully.");
                log?.AppendText("Resetting unit\r\n");
            }
            else
            {
                Logger.Log("⚠️ Reset command did not return expected response.");
            }
        }

        // This command just sends a sync command to the validator
        public bool SendSync(TextBox log = null)
        {
            Logger.Log("🔄 Sending SYNC command...");
            m_cmd.CommandData[0] = CCommands.SSP_CMD_SYNC;
            m_cmd.CommandDataLength = 1;

            if (!SendCommand(log))
            {
                Logger.Log("❌ SYNC failed (SendCommand).");
                return false;
            }

            if (CheckGenericResponses(log))
            {
                Logger.Log("✅ SYNC successful.");
                log?.AppendText("SYNC completed\r\n");
                return true;
            }

            Logger.Log("⚠️ SYNC response not OK.");
            return false;
        }

        // This function sets the protocol version in the validator to the version passed across. Whoever calls
        // this needs to check the response to make sure the version is supported.
        public void SetProtocolVersion(byte pVersion, TextBox log = null)
        {
            m_cmd.CommandData[0] = CCommands.SSP_CMD_HOST_PROTOCOL_VERSION;
            m_cmd.CommandData[1] = pVersion;
            m_cmd.CommandDataLength = 2;
            if (!SendCommand(log)) return;
        }

        // This function sends the command LAST REJECT CODE which gives info about why a note has been rejected. It then
        // outputs the info to a passed across textbox.
        public void QueryRejection(TextBox log)
        {
            // Configure the command to query the last rejection reason
            m_cmd.CommandData[0] = CCommands.SSP_CMD_LAST_REJECT_CODE;
            m_cmd.CommandDataLength = 1;

            // Send the command
            if (!SendCommand(log))
            {
                // This log line remains as it indicates a communication failure, not a rejection reason.
                Logger.Log("❌ QueryRejection failed (SendCommand).");
                return;
            }

            // Check the generic response for success
            if (CheckGenericResponses(log))
            {
                if (log == null) return;

                // Determine the rejection reason based on the response data
                string rejectionReason;
                switch (m_cmd.ResponseData[1])
                {
                    case 0x00: rejectionReason = "Note accepted"; break;
                    case 0x01: rejectionReason = "Note length incorrect"; break;
                    case 0x02: rejectionReason = "Invalid note"; break;
                    case 0x03: rejectionReason = "Invalid note"; break;
                    case 0x04: rejectionReason = "Invalid note"; break;
                    case 0x05: rejectionReason = "Invalid note"; break;
                    case 0x06: rejectionReason = "Channel inhibited"; break;
                    case 0x07: rejectionReason = "Second note inserted during read"; break;
                    case 0x08: rejectionReason = "Host rejected note"; break;
                    case 0x09: rejectionReason = "Invalid note"; break;
                    case 0x0A: rejectionReason = "Invalid note read"; break;
                    case 0x0B: rejectionReason = "Note too long"; break;
                    case 0x0C: rejectionReason = "Validator disabled"; break;
                    case 0x0D: rejectionReason = "Mechanism slow/stalled"; break;
                    case 0x0E: rejectionReason = "Strim attempt"; break;
                    case 0x0F: rejectionReason = "Fraud channel reject"; break;
                    case 0x10: rejectionReason = "No notes inserted"; break;
                    case 0x11: rejectionReason = "Invalid note read"; break;
                    case 0x12: rejectionReason = "Twisted note detected"; break;
                    case 0x13: rejectionReason = "Escrow time-out"; break;
                    case 0x14: rejectionReason = "Bar code scan fail"; break;
                    case 0x15: rejectionReason = "Invalid note read"; break;
                    case 0x16: rejectionReason = "Invalid note read"; break;
                    case 0x17: rejectionReason = "Invalid note read"; break;
                    case 0x18: rejectionReason = "Invalid note read"; break;
                    case 0x19: rejectionReason = "Incorrect note width"; break;
                    case 0x1A: rejectionReason = "Note too short"; break;
                    default: rejectionReason = $"Unknown rejection code: 0x{m_cmd.ResponseData[1]:X2}"; break;
                }

                // Log the reason in one single, comprehensive line.
                Logger.Log($"🔍 Note rejected: {rejectionReason}");
                log.AppendText(rejectionReason + "\r\n");
            }
        }

        // This function performs a number of commands in order to setup the encryption between the host and the validator.
        public bool NegotiateKeys(TextBox log = null)
        {
            Logger.Log("Negotiating keys with validator...");

            // make sure encryption is off
            m_cmd.EncryptionStatus = false;

            // send sync
            if (log != null) log.AppendText("Syncing... ");
            m_cmd.CommandData[0] = CCommands.SSP_CMD_SYNC;
            m_cmd.CommandDataLength = 1;

            if (!SendCommand(log))
            {
                Logger.Log("Sync command failed during key negotiation.");
                return false;
            }
            if (log != null) log.AppendText("Success");

            Logger.Log("Sync successful. Proceeding with key negotiation.");

            m_eSSP.InitiateSSPHostKeys(keys, m_cmd);

            // send generator
            m_cmd.CommandData[0] = CCommands.SSP_CMD_SET_GENERATOR;
            m_cmd.CommandDataLength = 9;
            if (log != null) log.AppendText("Setting generator... ");

            // Convert generator to bytes and add to command data.
            BitConverter.GetBytes(keys.Generator).CopyTo(m_cmd.CommandData, 1);

            if (!SendCommand(log))
            {
                Logger.Log("Sync command failed during key negotiation.");
                return false;
            }
            if (log != null) log.AppendText("Success\r\n");

            // send modulus
            m_cmd.CommandData[0] = CCommands.SSP_CMD_SET_MODULUS;
            m_cmd.CommandDataLength = 9;
            if (log != null) log.AppendText("Sending modulus... ");

            // Convert modulus to bytes and add to command data.
            BitConverter.GetBytes(keys.Modulus).CopyTo(m_cmd.CommandData, 1);

            if (!SendCommand(log))
            {
                Logger.Log("Sync command failed during key negotiation.");
                return false;
            }
            if (log != null) log.AppendText("Success\r\n");

            // send key exchange
            m_cmd.CommandData[0] = CCommands.SSP_CMD_REQUEST_KEY_EXCHANGE;
            m_cmd.CommandDataLength = 9;
            if (log != null) log.AppendText("Exchanging keys... ");

            // Convert host intermediate key to bytes and add to command data.
            BitConverter.GetBytes(keys.HostInter).CopyTo(m_cmd.CommandData, 1);


            if (!SendCommand(log))
            {
                Logger.Log("Sync command failed during key negotiation.");
                return false;
            }
            if (log != null) log.AppendText("Success\r\n");

            // Read slave intermediate key.
            keys.SlaveInterKey = BitConverter.ToUInt64(m_cmd.ResponseData, 1);

            m_eSSP.CreateSSPHostEncryptionKey(keys);

            // get full encryption key
            m_cmd.Key.FixedKey = 0x0123456701234567;
            m_cmd.Key.VariableKey = keys.KeyHost;

            if (log != null) log.AppendText("Keys successfully negotiated\r\n");

            Logger.Log("Keys successfully negotiated.");
            return true;
        }

        // This function uses the setup request command to get all the information about the validator.
        public void ValidatorSetupRequest(TextBox log = null)
        {
            try
            {
                Logger.Log("Starting ValidatorSetupRequest...");

                StringBuilder sbDisplay = new StringBuilder(1000);

                // send setup request
                m_cmd.CommandData[0] = CCommands.SSP_CMD_SETUP_REQUEST;
                m_cmd.CommandDataLength = 1;

                if (!SendCommand(log))
                {
                    Logger.Log("ValidatorSetupRequest: SendCommand failed.");
                    return;
                }

                // display setup request


                // unit type
                int index = 1;
                sbDisplay.Append("Unit Type: ");
                m_UnitType = (char)m_cmd.ResponseData[index++];
                switch (m_UnitType)
                {
                    case (char)0x00: sbDisplay.Append("Validator"); break;
                    case (char)0x03: sbDisplay.Append("SMART Hopper"); break;
                    case (char)0x06: sbDisplay.Append("SMART Payout"); break;
                    case (char)0x07: sbDisplay.Append("NV11"); break;
                    case (char)0x0D: sbDisplay.Append("TEBS"); break;
                    default: sbDisplay.Append("Unknown Type"); break;
                }

                // firmware
                sbDisplay.AppendLine();
                sbDisplay.Append("Firmware: ");

                sbDisplay.Append((char)m_cmd.ResponseData[index++]);
                sbDisplay.Append((char)m_cmd.ResponseData[index++]);
                sbDisplay.Append(".");
                sbDisplay.Append((char)m_cmd.ResponseData[index++]);
                sbDisplay.Append((char)m_cmd.ResponseData[index++]);

                sbDisplay.AppendLine();
                // country code.
                // legacy code so skip it.
                index += 3;

                // value multiplier.
                // legacy code so skip it.
                index += 3;

                // Number of channels
                sbDisplay.AppendLine();
                sbDisplay.Append("Number of Channels: ");
                m_NumberOfChannels = m_cmd.ResponseData[index++];
                sbDisplay.Append(m_NumberOfChannels);
                sbDisplay.AppendLine();

                // channel values.
                // legacy code so skip it.
                index += m_NumberOfChannels; // Skip channel values

                // channel security
                // legacy code so skip it.
                index += m_NumberOfChannels;

                // real value multiplier
                // (big endian)
                sbDisplay.Append("Real Value Multiplier: ");
                m_ValueMultiplier = m_cmd.ResponseData[index + 2];
                m_ValueMultiplier += m_cmd.ResponseData[index + 1] << 8;
                m_ValueMultiplier += m_cmd.ResponseData[index] << 16;
                sbDisplay.Append(m_ValueMultiplier);
                sbDisplay.AppendLine();
                index += 3;


                // protocol version
                sbDisplay.Append("Protocol Version: ");
                m_ProtocolVersion = m_cmd.ResponseData[index++];
                sbDisplay.Append(m_ProtocolVersion);
                sbDisplay.AppendLine();

                // Add channel data to list then display.
                // Clear list.
                m_UnitDataList.Clear();
                // Loop through all channels.

                for (byte i = 0; i < m_NumberOfChannels; i++)
                {
                    ChannelData loopChannelData = new ChannelData();
                    // Channel number.
                    loopChannelData.Channel = (byte)(i + 1);

                    // Channel value.
                    loopChannelData.Value = BitConverter.ToInt32(m_cmd.ResponseData, index + (m_NumberOfChannels * 3) + (i * 4)) * m_ValueMultiplier;

                    // Channel Currency
                    loopChannelData.Currency[0] = (char)m_cmd.ResponseData[index + (i * 3)];
                    loopChannelData.Currency[1] = (char)m_cmd.ResponseData[(index + 1) + (i * 3)];
                    loopChannelData.Currency[2] = (char)m_cmd.ResponseData[(index + 2) + (i * 3)];

                    // Channel level.
                    loopChannelData.Level = 0;

                    // Channel recycling
                    loopChannelData.Recycling = false;

                    // Add data to list.
                    m_UnitDataList.Add(loopChannelData);

                    //Display data
                    sbDisplay.Append("Channel ");
                    sbDisplay.Append(loopChannelData.Channel);
                    sbDisplay.Append(": ");
                    sbDisplay.Append(loopChannelData.Value / m_ValueMultiplier);
                    sbDisplay.Append(" ");
                    sbDisplay.Append(loopChannelData.Currency);
                    sbDisplay.AppendLine();
                }

                // Sort the list by .Value.
                m_UnitDataList.Sort((d1, d2) => d1.Value.CompareTo(d2.Value));

                if (log != null)
                    log.AppendText(sbDisplay.ToString());
                Logger.Log("ValidatorSetupRequest completed successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception in ValidatorSetupRequest", ex);
            }
        }

        // This function sends the set inhibits command to set the inhibits on the validator. An additional two
        // bytes are sent along with the command byte to indicate the status of the inhibits on the channels.
        // For example 0xFF and 0xFF in binary is 11111111 11111111. This indicates all 16 channels supported by
        // the validator are uninhibited. If a user wants to inhibit channels 8-16, they would send 0x00 and 0xFF.
        public void SetInhibits(TextBox log = null)
        {
            try
            {
                Logger.Log("Setting channel inhibits...");

                // set inhibits
                m_cmd.CommandData[0] = CCommands.SSP_CMD_SET_CHANNEL_INHIBITS;
                m_cmd.CommandData[1] = 0xFF;
                m_cmd.CommandData[2] = 0xFF;
                m_cmd.CommandDataLength = 3;


                if (!SendCommand(log))
                {
                    Logger.Log("SetInhibits: SendCommand failed.");
                    return;
                }

                if (CheckGenericResponses(log))
                {
                    Logger.Log("Inhibits set successfully.");
                    log?.AppendText("Inhibits set\r\n");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in SetInhibits", ex);
            }
        }

        // This function gets the serial number of the device.  An optional Device parameter can be used
        // for TEBS systems to specify which device's serial number should be returned.
        // 0x00 = NV200
        // 0x01 = SMART Payout
        // 0x02 = Tamper Evident Cash Box.
        public void GetSerialNumber(TextBox log = null)
        {
            try
            {
                Logger.Log("Getting serial number...");

                m_cmd.CommandData[0] = CCommands.SSP_CMD_GET_SERIAL_NUMBER;
                m_cmd.CommandDataLength = 1;

                if (!SendCommand(log))
                {
                    Logger.Log("GetSerialNumber: SendCommand failed.");
                    return;
                }

                if (CheckGenericResponses(log))
                {
                    Array.Reverse(m_cmd.ResponseData, 1, 4);
                    uint serial = BitConverter.ToUInt32(m_cmd.ResponseData, 1);

                    log?.AppendText($"Serial Number: {serial}\r\n");
                    Logger.Log($"Serial number is {serial}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in GetSerialNumber", ex);
            }
        }
        // The poll function is called repeatedly to poll to validator for information, it returns as
        // a response in the command structure what events are currently happening.

        private readonly object _countsLock = new object();

        internal Dictionary<string, int> _noteEscrowCounts = new Dictionary<string, int>();
        public IReadOnlyDictionary<string, int> NoteEscrowCounts
        {
            get
            {
                lock (_countsLock)
                {
                    return new Dictionary<string, int>(_noteEscrowCounts);
                }
            }
        }

        public event Action<string, int> NoteEscrowUpdated;

        public void ClearNoteEscrowCounts()
        {
            lock (_countsLock)
            {
                //Logger.Log("[CLEAR] Clearing note escrow counts.");
                _noteEscrowCounts.Clear();
            }
        }



        public bool DoPoll(TextBox log)
        {
            try
            {
                if (m_HoldCount > 0)
                {
                    m_NoteHeld = true;
                    m_HoldCount--;
                    m_cmd.CommandData[0] = CCommands.SSP_CMD_HOLD;
                    m_cmd.CommandDataLength = 1;
                    log?.AppendText($"Note held in escrow: {m_HoldCount}\r\n");
                    Logger.Debug($"Note held in escrow: {m_HoldCount}");

                    if (!SendCommand(log))
                        return false;

                    return true;
                }

                m_cmd.CommandData[0] = CCommands.SSP_CMD_POLL;
                m_cmd.CommandDataLength = 1;
                m_NoteHeld = false;

                if (!SendCommand(log))
                    return false;

                int noteVal = 0;
                for (byte i = 1; i < m_cmd.ResponseDataLength; i++)
                {
                    Logger.Log("m_cmd.ResponseData[i] -- " + m_cmd.ResponseData[i].ToString());
                    switch (m_cmd.ResponseData[i])
                    {
                        case CCommands.SSP_POLL_SLAVE_RESET:
                            log?.AppendText("Unit reset\r\n");
                            Logger.Log("Validator reset.");
                            break;

                        case CCommands.SSP_POLL_READ_NOTE:
                            if (m_cmd.ResponseData[i + 1] > 0)
                            {
                                noteVal = GetChannelValue(m_cmd.ResponseData[i + 1]);
                                string currency = GetChannelCurrency(m_cmd.ResponseData[i + 1]);
                                string formattedAmount = CHelpers.FormatToCurrency(noteVal);
                                string key = $"{formattedAmount} {currency}";

                                lock (_countsLock)
                                {
                                    bool existing = _noteEscrowCounts.ContainsKey(key);
                                    if (existing)
                                    {
                                        _noteEscrowCounts[key]++;
                                    }
                                    else
                                    {
                                        _noteEscrowCounts[key] = 1;
                                    }
                                }

                                OnNoteEscrowUpdated(key, _noteEscrowCounts[key]);  // 👈 This triggers event log
                                m_HoldCount = m_HoldNumber;
                                Logger.Log($"📥 Escrow: {formattedAmount} {currency} (count {_noteEscrowCounts[key]})");

                            }
                            else
                            {
                                log.AppendText("Reading note...\r\n");
                                Logger.Debug("Reading note...");
                            }
                            i++;
                            break;
                        case CCommands.SSP_POLL_CREDIT_NOTE:
                            noteVal = GetChannelValue(m_cmd.ResponseData[i + 1]);
                            string creditCurrency = GetChannelCurrency(m_cmd.ResponseData[i + 1]);
                            string formatted = CHelpers.FormatToCurrency(noteVal);

                            log?.AppendText($"Credit: {formatted} {creditCurrency}\r\n");
                            Logger.Log($"💵 Credit: {formatted} {creditCurrency}");
                            m_NumStackedNotes++;
                            i++;
                            break;

                        case CCommands.SSP_POLL_NOTE_REJECTED:
                            log?.AppendText("Note rejected\r\n");
                            Logger.Log("❌ Note rejected.");
                            QueryRejection(log);
                            break;

                        case CCommands.SSP_POLL_STACKER_FULL:
                            log?.AppendText("Stacker full\r\n");
                            Logger.Log("Stacker full.");
                            break;

                        case CCommands.SSP_POLL_FRAUD_ATTEMPT:
                            int fraudValue = GetChannelValue(m_cmd.ResponseData[i + 1]);
                            log?.AppendText($"Fraud attempt, note value: {fraudValue}\r\n");
                            Logger.Log($"Fraud attempt detected: value = {fraudValue}");
                            i++;
                            break;

                        case CCommands.SSP_POLL_NOTE_PATH_OPEN:
                            log?.AppendText("Note path open\r\n");
                            Logger.Log("Note path open.");
                            break;

                        case CCommands.SSP_POLL_CASHBOX_REMOVED:
                            log?.AppendText("Cashbox removed\r\n");
                            Logger.Log("Cashbox removed.");
                            break;

                        case CCommands.SSP_POLL_CASHBOX_REPLACED:
                            log?.AppendText("Cashbox replaced\r\n");
                            Logger.Log("Cashbox replaced.");
                            break;

                        default:
                            // Only log unknown responses in debug, not for client
                            Logger.Debug($"Unrecognized poll response: {m_cmd.ResponseData[i]}");
                            break;
                    }
                }

                DisplayNoteCounts(log);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in DoPoll", ex);
                return false;
            }
        }


        protected virtual void OnNoteEscrowUpdated(string key, int count)
        {
            if (NoteEscrowUpdated != null)
            {
                NoteEscrowUpdated?.Invoke(key, count);
            }
        }



        public void DisplayNoteCounts(TextBox log)
        {
            if (log == null || log.IsDisposed)
                return;

            log.AppendText("Note escrow summary:\r\n");
            foreach (var entry in _noteEscrowCounts)
            {
                log.AppendText($"{entry.Key} -> {entry.Value} times\r\n");
            }
        }


        /* Non-Command functions */

        // This function calls the open com port function of the SSP library.
        public bool OpenComPort(TextBox log = null)
        {
            try
            {
                Logger.Log("Attempting to open COM port...");

                log?.AppendText("Opening com port\r\n");

                if (!m_eSSP.OpenSSPComPort(m_cmd))
                {
                    Logger.Log("OpenComPort failed.");
                    return false;
                }

                Logger.Log("COM port opened successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in OpenComPort", ex);
                return false;
            }
        }


        /* Exception and Error Handling */

        // This is used for generic response error catching, it outputs the info in a
        // meaningful way.
        private bool CheckGenericResponses(TextBox log)
        {
            if (m_cmd.ResponseData[0] == CCommands.SSP_RESPONSE_OK)
                return true;
            else
            {
                if (log != null)
                {
                    switch (m_cmd.ResponseData[0])
                    {
                        case CCommands.SSP_RESPONSE_COMMAND_CANNOT_BE_PROCESSED:
                            if (m_cmd.ResponseData[1] == 0x03)
                            {
                                log.AppendText("Validator has responded with \"Busy\", command cannot be processed at this time\r\n");
                            }
                            else
                            {
                                log.AppendText("Command response is CANNOT PROCESS COMMAND, error code - 0x"
                                + BitConverter.ToString(m_cmd.ResponseData, 1, 1) + "\r\n");
                            }
                            return false;
                        case CCommands.SSP_RESPONSE_FAIL:
                            log.AppendText("Command response is FAIL\r\n");
                            return false;
                        case CCommands.SSP_RESPONSE_KEY_NOT_SET:
                            log.AppendText("Command response is KEY NOT SET, Validator requires encryption on this command or there is"
                                + "a problem with the encryption on this request\r\n");
                            return false;
                        case CCommands.SSP_RESPONSE_PARAMETER_OUT_OF_RANGE:
                            log.AppendText("Command response is PARAM OUT OF RANGE\r\n");
                            return false;
                        case CCommands.SSP_RESPONSE_SOFTWARE_ERROR:
                            log.AppendText("Command response is SOFTWARE ERROR\r\n");
                            return false;
                        case CCommands.SSP_RESPONSE_COMMAND_NOT_KNOWN:
                            log.AppendText("Command response is UNKNOWN\r\n");
                            return false;
                        case CCommands.SSP_RESPONSE_WRONG_NO_PARAMETERS:
                            log.AppendText("Command response is WRONG PARAMETERS\r\n");
                            return false;
                        default:
                            return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public bool SendCommand(TextBox log)
        {
            // Backup data and length in case we need to retry
            byte[] backup = new byte[255];
            m_cmd.CommandData.CopyTo(backup, 0);
            byte length = m_cmd.CommandDataLength;

            // attempt to send the command
            if (m_eSSP.SSPSendCommand(m_cmd, info) == false)
            {
                m_eSSP.CloseComPort();
                //m_Comms.UpdateLog(info, true); // update the log on fail as well
                if (log != null) log.AppendText("Sending command failed\r\nPort status: " + m_cmd.ResponseStatus.ToString() + "\r\n");
                return false;
            }

            // update the log after every command
            //m_Comms.UpdateLog(info);

            return true;
        }
    };
}
