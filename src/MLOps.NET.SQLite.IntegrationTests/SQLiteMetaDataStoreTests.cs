using FluentAssertions;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MLOps.NET.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MLOps.NET.SQLite.IntegrationTests
{
    [TestCategory("Integration")]
    [TestClass]
    public class SQLiteMetaDataStoreTests
    {
        [TestMethod]
        public async Task CreateExperimentAsync_Always_ReturnsNonEmptyGuidAsync()
        {
            //Arrange
            IMLOpsContext mlm = new MLOpsBuilder()
                .UseModelRepository(new Mock<IModelRepository>().Object)
                .UseSQLite()
                .Build();

            //Act
            var guid = await mlm.LifeCycle.CreateExperimentAsync("first experiment");

            //Assert
            Guid.TryParse(guid.ToString(), out var parsedGuid);
            parsedGuid.Should().NotBeEmpty();
        }

        [TestMethod]
        public async Task SetTrainingTimeAsync_SetsTrainingTimeOnRun()
        {
            //Arrange
            var unitUnderTest = new MLOpsBuilder()
                .UseSQLite()
                .UseModelRepository(new Mock<IModelRepository>().Object)
                .Build();
            var runId = await unitUnderTest.LifeCycle.CreateRunAsync("Test");

            var expectedTrainingTime = new TimeSpan(0, 5, 0);

            //Act
            await unitUnderTest.LifeCycle.SetTrainingTimeAsync(runId, expectedTrainingTime);

            //Assert
            var run = unitUnderTest.LifeCycle.GetRun(runId);
            run.TrainingTime.Should().Be(expectedTrainingTime);
        }

        [TestMethod]
        public async Task LogConfusionMatrixAsync_SavesConfusionMatrixOnRun()
        {
            //Arrange
            var unitUnderTest = new MLOpsBuilder()
                .UseSQLite()
                .UseModelRepository(new Mock<IModelRepository>().Object)
                .Build();
            var runId = await unitUnderTest.LifeCycle.CreateRunAsync("Test");
            var mlContext = new MLContext(seed: 2);
            List<DataPoint> samples = GetSampleDataForTraining();

            var data = mlContext.Data.LoadFromEnumerable(samples);
            var trainer = mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(labelColumnName: "Label", featureColumnName: "Features");

            var model = trainer.Fit(data);

            var predicitions = model.Transform(data);
            var metrics = mlContext.BinaryClassification.Evaluate(predicitions, labelColumnName: "Label");

            //Act
            await unitUnderTest.Evaluation.LogConfusionMatrixAsync(runId, metrics.ConfusionMatrix);

            //Assert
            var confusionMatrix = unitUnderTest.Evaluation.GetConfusionMatrix(runId);
            confusionMatrix.Should().NotBeNull();
            confusionMatrix.SerializedDetails.Should().NotBeNullOrEmpty();
        }

            [TestMethod]
        public void SetTrainingTimeAsync_NoRunProvided_ThrowsException()
        {
            //Arrange
            var unitUnderTest = new MLOpsBuilder()
                .UseSQLite()
                .UseModelRepository(new Mock<IModelRepository>().Object)
                .Build();

            var expectedTrainingTime = new TimeSpan(0, 5, 0);

            //Act and Assert
            var runId = Guid.NewGuid();
            var expectedMessage = $"The run with id {runId} does not exist";

            Func<Task> func = new Func<Task>(async () => await unitUnderTest.LifeCycle.SetTrainingTimeAsync(runId, expectedTrainingTime));

            func.Should().Throw<InvalidOperationException>(expectedMessage);
        }

        private static List<DataPoint> GetSampleDataForTraining()
        {
            return new List<DataPoint>()
            {
                new DataPoint(){ Features = new float[3] {0, 2, 1} , Label = false },
                new DataPoint(){ Features = new float[3] {0, 2, 3} , Label = false },
                new DataPoint(){ Features = new float[3] {0, 2, 4} , Label = true  },
                new DataPoint(){ Features = new float[3] {0, 2, 1} , Label = false },
                new DataPoint(){ Features = new float[3] {0, 2, 2} , Label = false },
                new DataPoint(){ Features = new float[3] {0, 2, 3} , Label = false },
                new DataPoint(){ Features = new float[3] {0, 2, 4} , Label = true  },
                new DataPoint(){ Features = new float[3] {1, 0, 0} , Label = true  }
            };
        }       
    }

    internal class DataPoint
    {
        public DataPoint()
        {
        }

        [VectorType(3)]
        public float[] Features { get; set; }

        public bool Label { get; set; }
    }
}
