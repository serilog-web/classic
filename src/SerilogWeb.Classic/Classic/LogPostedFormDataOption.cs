namespace SerilogWeb.Classic
{
    /// <summary>
    /// Describes options for logging form request data
    /// </summary>
    public enum LogPostedFormDataOption
    {
        /// <summary>
        /// Posted form values are never logged
        /// </summary>
        Never,
        /// <summary>
        /// Posted form values are always logged
        /// </summary>
        Always,
        /// <summary>
        /// Posted form values are logged if Response.StatusCode >= 500
        /// </summary>
        OnlyOnError,
#pragma warning disable 618
        /// <summary>
        /// Uses the custom predicate defined by <see cref="ApplicationLifecycleModule.ShouldLogPostedFormData"/>
        /// to determine if posted form values are logged
        /// </summary>
        OnMatch
#pragma warning restore 618
    }
}