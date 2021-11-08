using vcsparser.core.Database.Cosmos;

namespace vcsparser.core.Helpers
{
    public static class DocumentTypeHelper
    {
        public static DocumentType GetDocumentType<T>()
        {
            return typeof(T) == typeof(DailyCodeChurn) ? DocumentType.CodeChurn : DocumentType.BugDatabase;
        }
    }
}
