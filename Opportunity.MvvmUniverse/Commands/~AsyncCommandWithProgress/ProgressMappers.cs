using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Opportunity.MvvmUniverse.Commands
{
    internal static class ProgressMappers
    {
        public static double HttpProgressMapper<T>(IAsyncCommandWithProgress<T, HttpProgress> command, T parameter, HttpProgress progress)
            => HttpProgressMapper(command, progress);
        public static double HttpProgressMapper(ICommandWithProgress<HttpProgress> command, HttpProgress progress)
        {
            if (progress.Stage < HttpProgressStage.SendingHeaders)
                return double.NaN;
            var p = 0d;
            switch (progress.Stage)
            {
            case HttpProgressStage.SendingHeaders:
            case HttpProgressStage.SendingContent:
                if (progress.TotalBytesToSend is ulong toSend)
                    p = 0.2 * progress.BytesSent / toSend;
                else if (progress.BytesSent > 1_000)
                    p = 0.2 * (Math.Atan(progress.BytesSent - 1_000) / Math.PI * 2);
                else
                    p = 0;
                break;
            case HttpProgressStage.WaitingForResponse:
                p = 0.2;
                break;
            case HttpProgressStage.ReceivingHeaders:
            case HttpProgressStage.ReceivingContent:
                if (progress.TotalBytesToReceive is ulong toReceive)
                    p = 0.2 + 0.8 * progress.BytesReceived / toReceive;
                else if (progress.BytesReceived > 1_000)
                    p = 0.2 + 0.8 * (Math.Atan(progress.BytesReceived - 1_000) / Math.PI * 2);
                else
                    p = 0.2;
                break;
            }
            switch (progress.Retries)
            {
            case 0:
                return p;
            case 1:
                return 0.9 + 0.1 * p;
            case 2:
                return 0.95 + 0.05 * p;
            default:
                return 0.98 + 0.02 * p;
            }
        }
    }
}
