using ManageMyMusic.ExtractFile.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManageMyMusic
{
    public class Actions : IActions
    {
        private readonly IExcuteExtractFile m_ExcuteExtractFile;

        public Actions(IExcuteExtractFile excuteExtractFile)
        {
            m_ExcuteExtractFile = excuteExtractFile;
        }

        public Task DoActionsAsync()
        {
            m_ExcuteExtractFile.GetAllZipFilesPath();

            return Task.CompletedTask;
        }
    }
}
