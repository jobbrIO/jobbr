using System.Collections.Generic;
using System.IO;
using Jobbr.ComponentModel.ArtefactStorage.Model;

namespace Jobbr.ComponentModel.ArtefactStorage
{
    public interface IArtefactsStorageProvider
    {
        /// <summary>
        /// Stores a file uniquely identifyable by the container and the filename
        /// </summary>
        /// <param name="container">Usually relates to a JobRun and contains one or multiple files</param>
        /// <param name="fileName">The original fileName that was used when creating the file</param>
        /// <param name="content">Stream to access the contents of the file</param>
        void Save(string container, string fileName, Stream content);

        /// <summary>
        /// Access a file's contents by providing the container and fileName as unique keys
        /// </summary>
        /// <param name="container">Usually relates to a JobRun and contains one or multiple files</param>
        /// <param name="fileName">The original fileName that was used when creating the file</param>
        /// <returns>A stream the gives access to the file-contents</returns>
        Stream Load(string container, string fileName);

        /// <summary>
        /// Provides access to all artefacts/files in a given container
        /// </summary>
        /// <param name="container">Usually relates to a JobRun and contains one or multiple files</param>
        /// <returns>A list of <see cref="JobbrArtefact"/>s</returns>
        List<JobbrArtefact> GetArtefacts(string container);
    }
}
