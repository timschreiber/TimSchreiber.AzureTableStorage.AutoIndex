using System;

namespace TimSchreiber.AzureTableStorage.AutoIndex.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AutoIndexAttribute : Attribute
    { }
}
