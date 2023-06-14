using ShapeDiver.SDK;
using PDTO = ShapeDiver.SDK.PlatformBackend.DTO;
using GDTO = ShapeDiver.SDK.GeometryBackend.DTO;
using ShapeDiver.SDK.PlatformBackend;
using ShapeDiver.SDK.GeometryBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetSdkSampleConsoleApp.Util
{
    internal static class GeometryBackendUtils
    {
        /// <summary>
        /// Wait for the model status to become 'confirmed', 'denied', or 'pending'
        /// </summary>
        /// <param name="sdk"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static async Task<GDTO.GeometryBackendResultDto> WaitForModelCheck(IShapeDiverSDK sdk, IGeometryBackendContext context)
        {
            var modelDto = context.ModelData;

            // check current status, must be one of NotUploaded, Uploaded, Pending
            if (modelDto.Model.Status != GDTO.ModelStatusEnum.NotUploaded &&
                modelDto.Model.Status != GDTO.ModelStatusEnum.Uploaded)
                return modelDto;

            // wait for upload to be detected
            var start = DateTime.UtcNow;
            while (modelDto.Model.Status == GDTO.ModelStatusEnum.NotUploaded)
            {
                if ((DateTime.UtcNow - start).TotalSeconds > 60)
                {
                    throw new Exception("Model checking did not start within 60 seconds");
                }
                Console.WriteLine($"Waiting for model check to start...");
                Thread.Sleep(2500);
                modelDto = await sdk.GeometryBackendClient.GetModel(context);
            }

            var maxCompTime = modelDto.Setting.Computation.MaxCompTime;
            Console.WriteLine($"Maximum allowed computation time: {maxCompTime/1000} seconds");

            // wait for model checking
            start = DateTime.UtcNow;
            while (modelDto.Model.Status != GDTO.ModelStatusEnum.Confirmed &&
                modelDto.Model.Status != GDTO.ModelStatusEnum.Denied &&
                modelDto.Model.Status != GDTO.ModelStatusEnum.Pending)
            {
                if ((DateTime.UtcNow - start).TotalMilliseconds > 2 * maxCompTime)
                {
                    throw new Exception($"Model check did not complete within ${maxCompTime/1000} seconds");
                }
                Console.WriteLine($"Waiting for model check to finish...");
                Thread.Sleep(2500);
                modelDto = await sdk.GeometryBackendClient.GetModel(context);
            }

            return modelDto;
        }

    }
}
