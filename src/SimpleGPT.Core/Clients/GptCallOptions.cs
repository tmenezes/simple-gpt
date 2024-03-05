using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGPT.Core.Clients
{
    public class GptCallOptions
    {
        /// <summary>
        ///     Gets or sets a value that influences the probability of generated tokens appearing based on their
        ///     cumulative frequency in generated text.
        ///     Has a valid range of -2.0 to 2.0.
        /// </summary>
        /// <remarks>
        ///     Positive values will make tokens less likely to appear as their frequency increases and decrease the
        ///     model's likelihood of repeating the same statements verbatim.
        /// </remarks>
        public float? FrequencyPenalty { get; set; }

        /// <summary> Gets the top answers </summary>
        public int? Top { get; set; }

        /// <summary> Gets the maximum number of tokens to generate. Has minimum of 0. </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        ///     Gets or sets a value that influences the probability of generated tokens appearing based on their
        ///     existing presence in generated text.
        ///     Has a valid range of -2.0 to 2.0.
        /// </summary>
        /// <remarks>
        ///     Positive values will make tokens less likely to appear when they already exist and increase the
        ///     model's likelihood to output new topics.
        /// </remarks>
        public float? PresencePenalty { get; set; }

        /// <summary>
        ///     Gets or sets the sampling temperature to use that controls the apparent creativity of generated
        ///     completions.
        ///     Has a valid range of 0.0 to 2.0 and defaults to 1.0 if not otherwise specified.
        /// </summary>
        /// <remarks>
        ///     Higher values will make output more random while lower values will make results more focused and
        ///     deterministic.
        /// </remarks>
        public float? Temperature { get; set; }
    }
}
