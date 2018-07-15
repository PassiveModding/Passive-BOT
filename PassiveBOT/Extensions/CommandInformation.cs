namespace PassiveBOT.Extensions
{
    using Discord.Commands;

    /// <summary>
    /// command info manipulation
    /// </summary>
    public class CommandInformation
    {
        /// <summary>
        /// The parameter information.
        /// </summary>
        /// <param name="parameter">
        /// The param.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ParameterInformation(ParameterInfo parameter)
        {
            var initial = parameter.Name;
            var isAttributed = false;
            if (parameter.IsOptional)
            {
                initial = $"[{initial} = {parameter.DefaultValue}]";
                isAttributed = true;
            }

            if (parameter.IsMultiple)
            {
                initial = $"|{initial}|";
                isAttributed = true;
            }

            if (parameter.IsRemainder)
            {
                initial = $"...{initial}";
                isAttributed = true;
            }
            
            if (!isAttributed)
            {
                initial = $"<{initial}>";
            }

            return initial;
        }
    }
}
