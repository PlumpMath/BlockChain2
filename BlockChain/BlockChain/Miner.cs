﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptSharp.Utility;
using System.Threading;

namespace BlockChain
{
    class Miner
    {
        public static void Scrypt(CBlock block) //TODO: implementare evento per l'uscita in caso sia stato trovato un blocco parallelo
        {

            string toHash;
            string hash;
            bool found = false;
            int higher = 0, current = 0;

            while (!found)
            {
                block.Timestamp = DateTime.Now;
                toHash = block.Header.PreviousBlockHash + block.Nonce + block.Timestamp + block.MerkleRoot; //si concatenano vari parametri del blocco TODO: usare i parmetri giusti, quelli usati qua sono solo per dimostrazione e placeholder
                hash = Utilities.ByteArrayToString(SCrypt.ComputeDerivedKey(Encoding.ASCII.GetBytes(toHash), Encoding.ASCII.GetBytes(toHash), 1024, 1, 1, 1, 32)); //calcola l'hash secondo il template di scrypt usato da litecoin
                if (Program.DEBUG)
                    CIO.DebugOut("Hash corrente blocco " + block.Header.BlockNumber + ": " + hash);
                for (int i = 0; i <= block.Difficulty; i++)
                {
                    if (i == block.Difficulty) //se il numero di zeri davanti la stringa è pari alla difficoltà del blocco, viene settato l'hash e si esce
                    {
                        block.Header.Hash = hash;
                        CBlockChain.Instance.Add(new CTemporaryBlock(block,null));
                        CPeers.Instance.DoRequest(ERequest.BroadcastMinedBlock, block);
                        Thread.Sleep(200);
                        return;
                    }
                    if (!(hash[i] == '0'))
                    {
                        current = 0;
                        break;
                    }

                    current++;
                    if (higher < current)
                    {
                        higher = current;
                    }

                }

                block.Nonce++; //incremento della nonce per cambiare hash
            }

        }

        public static bool Verify(CBlock block)
        {
            string toHash = block.Header.PreviousBlockHash + block.Nonce + block.Timestamp + block.MerkleRoot;
            if (block.Header.Hash == Utilities.ByteArrayToString(SCrypt.ComputeDerivedKey(Encoding.ASCII.GetBytes(toHash), Encoding.ASCII.GetBytes(toHash), 1024, 1, 1, 1, 32)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
