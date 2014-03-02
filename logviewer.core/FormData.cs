// Created by: egr
// Created at: 13.10.2013
// © 2012-2014 Alexander Egorov

namespace logviewer.core
{
    public class FormData
    {
        public string KeepLastNFiles { get; set; }
        public bool OpenLastFile { get; set; }
        public bool AutoRefreshOnFileChange { get; set; }
        public string PageSize { get; set; }
    }
}