using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using LanPartyTool.config;
using LanPartyTool.utility;
using Microsoft.CSharp.RuntimeBinder;

namespace LanPartyTool.agent
{
    public class Executor
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Executor));

        private readonly dynamic _command;

        private readonly Config _config = Config.GetInstance();

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
                cfgLines = _command.cfgLines.ToObject<List<string>>();
            }
            catch (RuntimeBinderException e)
            {
                Logger.Warn(e.Message);
                return JsonResponse.GetErrorInstance(e.Message);
            }

            if (cfgLines == null || cfgLines.Count == 0)
            {
                Logger.Warn("No CFG lines in command");
                return JsonResponse.GetErrorInstance("No CFG lines");
            }

            Logger.Debug($"New CFG with {cfgLines.Count} lines");

            Logger.Debug("Updating tool CFG");
            GameUtility.WriteCfg(_config.ToolCfg, cfgLines);

            Thread.Sleep(1000);

            Logger.Debug("Triggering keyboard keypress");
            WindowsUtility.SendKeyDown(Constants.GameExeName);

            return JsonResponse.GetSuccessInstance();
        }
    }
}