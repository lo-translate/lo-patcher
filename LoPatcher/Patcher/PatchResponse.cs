using System;
using System.Collections.Generic;

namespace LoPatcher.Patcher
{
    public class PatchResponse
    {
        public bool Success { get; set; }

        public int FilesPatched { get; set; }

        public List<string> Errors { get; } = new List<string>();

        public PatchResponse()
        {
        }

        public PatchResponse(Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            Errors.Add(error.Message);
        }
    }
}