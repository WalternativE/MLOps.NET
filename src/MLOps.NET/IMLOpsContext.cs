﻿using MLOps.NET.Catalogs;

namespace MLOps.NET
{
    /// <summary>
    /// Access point to manage the lifecycle of a machin learning model
    /// </summary>
    public interface IMLOpsContext
    {
        /// <summary>
        /// Operations related to the lifecycle of a model
        /// </summary>
        LifeCycleCatalog LifeCycleCatalog { get; set; }

        /// <summary>
        /// Operations related to tracking the evaulation metrics of a model
        /// </summary>
        EvaluationCatalog Evaluation { get; set; }

        /// <summary>
        /// Operations related to a model
        /// </summary>
        ModelCatalog Model { get; set; }

        /// <summary>
        /// Operations related to the training of a model
        /// </summary>
        TrainingCatalog Training { get; set; }
    }
}
