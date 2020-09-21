using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace FTPclient
{
    public delegate void EventHandler();//declarando o delegate

    class connectionFTPdata
    {
        public static event EventHandler OnDowndload;//Criando o evento
        public string URL; //A URL vai ser o endereço ftp e será a pasta base para o download dos arquivos
        public string Username; //Nome do usuário que vai ser validado na conexão
        public string Password; // Senha do usuário
        public FileInfo ArquivoUpload; // Facilita a manipulação do arquivo e evita de ficar passando parâmetros a todo método criado
        public string ArquivoDownload; // Facilita a manipulação do arquivo e evita de ficar passando parâmetros a todo método criado

        #region Listar diretorio do FTP
        //Lista os arquivos dentro do diretório.
        public void ListarDiretorioFTP()
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(URL);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                request.Credentials = new NetworkCredential(Username, Password);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string names = reader.ReadToEnd();
                Console.WriteLine(names);

                
                reader.Close();
                response.Close();
            }
            catch (Exception)
            {
                OnDowndload = OnError;
                OnDowndload.Invoke();
            }
        }
        #endregion

        #region Baixar arquivo do ftp
        //A sting Local armazena o nome do arquivo que você quer baixar na pasta raiz, vulgo a URL
        //os arquivos são baixados passando o nome e o tipo como parâmetro exp: "teste.txt"
        public void BaixarArquivoFTP()
        {
            try
            {
                OnDowndload = OnInit;
                OnDowndload.Invoke();
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(URL+ArquivoDownload);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                request.Credentials = new NetworkCredential(Username, Password);
                var response = (FtpWebResponse)request.GetResponse();

                var responseStream = response.GetResponseStream();
                if (FileDirectoryExists() == true)
                {
                    throw new Exception();
                }
                else
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        responseStream.CopyTo(memoryStream);
                        var conteudoArquivo = memoryStream.ToArray();
                        File.WriteAllBytes(ArquivoDownload, conteudoArquivo);
                    }
                    response.Close();
                    //MoverFilePara();
                    OnDowndload = OnComplete;
                    OnDowndload.Invoke();
                }
            }
            catch (Exception ex)
            {
                if(FileDirectoryExists() == true)
                {
                    OnDowndload = OnFileExists;
                    OnDowndload += OnError;
                    OnDowndload.Invoke();
                }
                else
                {
                    OnDowndload = OnError;
                    OnDowndload.Invoke();
                }
            }
        }
        #endregion

        #region subindo arquivo para o ftp
        //Para poder fazer o upload do arquivo é necessário passar o caminho todo do arquivo Ex: @"c:\path\testeenvio.txt"
        //Dentro do método tem um objeto "FileInfo", onde trata o documento para facilitar a manipulação do mesmo
        public void uploadFTP()
        {
            try
            {
                if (FTPfileExists() == true)
                {
                    throw new Exception();
                }
                else
                {
                    var uri = new Uri(URL + ArquivoUpload.Name);
                    using(var webClient = new WebClient())
                    {
                        webClient.Credentials = new NetworkCredential(Username, Password);
                        webClient.UploadFile(uri, ArquivoUpload.FullName);
                    }
                    OnDowndload = OnUploadComplete;
                    OnDowndload.Invoke();
                }
            }
            catch(Exception ex)
            {
                if (FTPfileExists() == true)
                {
                    OnDowndload = OnUploadExists;
                    OnDowndload.Invoke();
                }
                else
                {
                    OnDowndload = OnUploadErro;
                    OnDowndload.Invoke();
                }
            }
            
        }
        #endregion

        #region Checar Existencia do arquivo na pasta ftp

        public bool FTPfileExists()
        {
            try
            {

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(URL + ArquivoUpload.Name);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            request.Credentials = new NetworkCredential(Username, Password);
            WebResponse Response = request.GetResponse();

            StreamReader reader = new StreamReader(Response.GetResponseStream());
            bool ExisteArquivo = reader.ReadToEnd().Contains(ArquivoUpload.Name);
            return ExisteArquivo;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        static bool FileDirectoryExists()
        {
            if (File.Exists(@"C: \path\teste.txt") == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Funções para o invoke
        static void OnInit()
        {
            Console.WriteLine("O download foi iniciado");
        }

        static void OnComplete()
        {
            Console.WriteLine("O download foi completado");
        }

        static void OnUploadComplete()
        {
            Console.WriteLine("O Upload foi completado");
        }

        static void OnUploadErro()
        {
            Console.WriteLine("O Upload foi completado");
        }

        static void OnUploadExists()
        {
            Console.WriteLine("O arquivo já existe");
        }

        static void OnError()
        {
            Console.WriteLine("O download não pode ser efetuado");
        }

        static void OnFileExists()
        {
            Console.WriteLine("O arquivo já existe");
                        
        }
        #endregion

        #region Move a file para o diretório especificado
        public void MoverFilePara()
        {
            string pathorigem = @"C:\origem\"+ArquivoDownload; //variavel para mover o arquivo
            string pathdestino = @"C:\destino\"+ArquivoDownload; //variavel para mover o arquivo
            File.Move(pathorigem, pathdestino); //sem essa linha o app coloca o arquivo na pasta raiz do projeto
        }
        #endregion
    }
}
