using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WavesCS;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WpfWavesApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Use faucet to fill accounts https://testnet.wavesexplorer.com/faucet
        Node node = new Node();

        public PrivateKeyAccount account;
        public string seed;
        public string base58PrivateKey;
        public string base58PublicKey;
        public void Init()
        {
            Seed = PrivateKeyAccount.GenerateSeed();
            Account = PrivateKeyAccount.CreateFromSeed(seed, AddressEncoding.TestNet);
            Base58PrivateKey = Account.PrivateKey.ToBase58();
            Base58PublicKey = Account.PublicKey.ToBase58();
        }
      
        public PrivateKeyAccount Account
        {
            get { return account; }
            set
            {
                account = value;
                OnPropertyChanged("Account");
            }
        }

        public string Seed
        {
            get { return seed; }
            set
            {
                seed = value;
                OnPropertyChanged("Seed");
            }
        }
        public string Base58PrivateKey
        {
            get { return base58PrivateKey; }
            set
            {
                base58PrivateKey = value;
                OnPropertyChanged("Base58PrivateKey");
            }
        }
        public string Base58PublicKey
        {
            get { return base58PublicKey; }
            set
            {
                base58PublicKey = value;
                OnPropertyChanged("Base58PublicKey");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private RelayCommand addCommand;
        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ??
                    (addCommand = new RelayCommand(obj => Init()));
            }
        }

        public void Load()
        {    
            using (StreamReader sr = new StreamReader("account.dat"))
            {                
                Seed = sr.ReadLine();
                Account = PrivateKeyAccount.CreateFromSeed(Seed, AddressEncoding.TestNet);
            }
        }

        private RelayCommand saveData;
        public RelayCommand SaveData
        {
            get
            {
                return saveData ??
                    (saveData = new RelayCommand(obj => Save()));
            }
        }

        public void Save()
        {            
            using(StreamWriter sw = new StreamWriter("account.dat"))
            {
                sw.WriteLine(Seed);
                string json = JsonConvert.SerializeObject(account, Formatting.Indented);
                sw.WriteLine(json);                
            }         
        }

        private RelayCommand loadData;
        public RelayCommand LoadData
        {
            get
            {
                return loadData ??
                    (loadData = new RelayCommand(obj => Load()));
            }
        }

        private RelayCommand sendDataCommand;
        public RelayCommand SendDataCommand
        {
            get
            {
                return sendDataCommand ??
                    (sendDataCommand = new RelayCommand(obj => SendData()));
            }
        }

        public void SendData()
        {
            var data = new DictionaryObject
            {
                { "Laba", 1L },
                { "Check", true },
                { "Work", new byte[] { 1, 2, 3, 4, 5}},
                { "Name", "Roman"}
            };

            var tx = new DataTransaction(node.ChainId, Account.PublicKey, data).Sign(Account);
            node.BroadcastAndWait(tx.GetJsonWithSignature());
            var addressData = node.GetAddressData(Account.Address);
            MessageBox.Show($"Laba: {addressData["Laba"]} \nCheck: {addressData["Check"]}\nWork: {addressData["Work"]}\nName: {addressData["Name"]}");
        }
        public void MassTransferSend()
        {      
            var recipients = new List<MassTransferItem>
            {
                new MassTransferItem("3N1JMgUfzYUZinPrzPWeRa6yqN67oo57XR1", 0.001m),
                new MassTransferItem("3N1JMgUfzYUZinPrzPWeRa6yqN67oo57XR2", 0.002m),
                new MassTransferItem("3N1JMgUfzYUZinPrzPWeRa6yqN67oo57XR3", 0.003m),
            };
            var tx = new MassTransferTransaction(node.ChainId, Account.PublicKey, Assets.WAVES, recipients, "Shut up & take my money");                  
            tx.Sign(Account);
            node.BroadcastAndWait(tx.GetJsonWithSignature());
        }
    }
}
