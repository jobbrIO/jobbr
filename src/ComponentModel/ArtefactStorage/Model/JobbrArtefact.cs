namespace Jobbr.ComponentModel.ArtefactStorage.Model
{
    /// <summary>
    /// A JobbrArtefact encapsulates one file that has been generated while the job was run.
    /// It's the responsibility of the provider to store this metainformation along the file contents
    /// </summary>
    public class JobbrArtefact
    {
        /// <summary>
        /// Gets or sets the original fileName of the file when it was created by the job
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or setzs the MimeType of the created file
        /// </summary>
        public string MimeType { get; set; }
        
        /// <summary>
        /// Gets or sets the size of the file in bytes
        /// </summary>
        public long Size { get; set; }
    }
}