using System;
using System.Collections.Generic;
using log4net;
using Microsoft.CSharp.RuntimeBinder;

namespace LanPartyTool.agent
{
    public class Executor
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Executor));

        private readonly dynamic _command;

        public Executor(dynamic command)
        {
            _command = command;
        }

        public JsonResponse Execute()
        {
            switch (Convert.ToString(_command.action))
            {
                case Protocol.Command.Ping:
                    return DoPing();
                case Protocol.Command.WriteCfg:
                    return WriteCfg();
                default:
                    return JsonResponse.GetErrorInstance("Unknown command");
            }
        }

        private JsonResponse DoPing()
        {
            Logger.Debug("Ping command");
            return JsonResponse.GetSuccessInstance(_command);
        }

        private JsonResponse WriteCfg()
        {
            Logger.Debug("WriteCfg command");

            List<string> cfgLines;

            try
            {
                cfgLines = _command.cfgLines;
            }
            catch (RuntimeBinderException)
            {
                return JsonResponse.GetErrorInstance("No CFG lines");
            }

            Logger.Debug(cfgLines);

            return JsonResponse.GetSuccessInstance();
        }
    }
}