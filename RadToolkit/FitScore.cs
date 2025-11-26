namespace Skyline.DataMiner.Utils.RadToolkit
{
    /// <summary>
    /// A fit score for a relational anomaly subgroup.
    /// </summary>
    public class FitScore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FitScore"/> struct.
        /// </summary>
        /// <param name="modelFit">The model fit score of the subgroup.</param>
        /// <param name="isOutlier">Whether the subgroup is an outlier.</param>
        public FitScore(double modelFit, bool isOutlier)
        {
            ModelFit = modelFit;
            IsOutlier = isOutlier;
        }

        /// <summary>
        /// Gets or sets the model fit score of the subgroup. See also <see cref="Skyline.DataMiner.Analytics.Rad.RADSubgroupFitScore.ModelFit"/> for more information.
        /// </summary>
        public double ModelFit { get; set; }

        /// <summary>
        /// Gets or sets whether the subgroup is an outlier. See also <see cref="Skyline.DataMiner.Analytics.Rad.RADSubgroupFitScore.IsOutlier"/> for more information.
        /// </summary>
        public bool IsOutlier { get; set; }
    }
}
