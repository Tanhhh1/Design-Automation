using Autodesk.Forge.DesignAutomation;
using Autodesk.Forge.DesignAutomation.Model;
using DesignAutomation.Models.Activity;

namespace DesignAutomation.Services
{
    public class ActivityService
    {
        private readonly DesignAutomationClient _daClient;

        public ActivityService(DesignAutomationClient daClient)
        {
            _daClient = daClient;
        }
        public async Task<Activity> CreateActivityAsync(ActivityRequest request)
        {
            var activity = new Activity
            {
                Id = request.id,
                Engine = request.engine,
                CommandLine = request.commandLine,
                Appbundles = request.appBundles,
                Parameters = new Dictionary<string, Parameter>()
                //Settings = new Dictionary<string, ISetting>()
            };
            /*if (request.settings != null)
            {
                foreach (var setting in request.settings)
                {
                    activity.Settings.Add(
                        setting.Key,
                        new StringSetting
                        {
                            Value = setting.Value
                        }
                    );
                }
            }*/
            if (request.parameters != null)
            {
                foreach (var item in request.parameters)
                {
                    if (Enum.TryParse(item.Value.Verb, true, out Verb verbEnum)) //chuyển chuỗi thành enum
                    {
                        activity.Parameters.Add(item.Key, new Parameter
                        {
                            Verb = verbEnum,
                            Description = item.Value.Description,
                            LocalName = item.Value.LocalName,
                            Required = item.Value.Required,
                            Zip = item.Value.Zip,
                            Ondemand = item.Value.Ondemand
                        });
                    }
                }
            }
            return await _daClient.CreateActivityAsync(activity);
        }

        public async Task<Alias> CreateAliasAsync(string id, ActivityAliasRequest request)
        {
            var alias = new Alias
            {
                Id = request.id,
                Version = request.version
            };
            return await _daClient.CreateActivityAliasAsync(id, alias);
        }

        public async Task<Activity> CreateVersionAsync(string id, ActivityVersionRequest request)
        {
            var newVersion = new Activity
            {
                Engine = request.engine,
                Appbundles = request.appBundles,
                CommandLine = request.commandLine,
                Parameters = new Dictionary<string, Parameter>()
                //Settings = new Dictionary<string, ISetting>()
            };
            /*if (request.settings != null)
            {
                foreach (var setting in request.settings)
                {
                    newVersion.Settings.Add(
                        setting.Key,
                        new StringSetting
                        {
                            Value = setting.Value
                        }
                    );
                }
            }*/
            if (request.parameters != null)
            {
                foreach (var item in request.parameters)
                {
                    if (Enum.TryParse(item.Value.Verb, true, out Verb verbEnum)) 
                    {
                        newVersion.Parameters.Add(item.Key, new Parameter
                        {
                            Verb = verbEnum,
                            Description = item.Value.Description,
                            LocalName = item.Value.LocalName,
                            Required = item.Value.Required,
                            Zip = item.Value.Zip,
                            Ondemand = item.Value.Ondemand
                        });
                    }
                }
            }
            return await _daClient.CreateActivityVersionAsync(id, newVersion);
        }

        public async Task UpdateAliasAsync(string id, string aliasId, UpdateActivityAliasRequest request)
        {
            var aliasUpdate = new AliasPatch { Version = request.version };
            await _daClient.ModifyActivityAliasAsync(id, aliasId, aliasUpdate);
        }
    }
}
