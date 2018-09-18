namespace p4codechurn.core
{
    internal class FileImpl : IFile
    {
        private string fullName;

        public FileImpl(string fullName)
        {
            this.fullName = fullName;
        }

        public string FileName
        {
            get { return fullName; }
        }        
    }
}