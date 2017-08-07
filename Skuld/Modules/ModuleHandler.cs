using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Discord;
using System.Linq;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace Skuld.Modules
{
    public class ModuleHandler
    {
        static readonly List<Discord.Commands.ModuleInfo> Modules = new List<Discord.Commands.ModuleInfo>();
        static string CurrentDir = Directory.GetCurrentDirectory()+@"\modules\";
        public static async Task LoadAll()
        {
            if (await CheckDir())
                foreach (string mod in Directory.GetFiles(CurrentDir))
                    await LoadModule(mod);
            else
                return;
        }
        public static async Task ReloadAll()
        {
            if (await CheckDir())
            {
                await UnloadAll();
                await LoadAll();
            }
            else
                return;
        }
        public static async Task UnloadAll()
        {
            string currentmod = null;
            if (await CheckDir())
            {
                try
                {
                    Modules.AddRange(Bot.commands.Modules.ToList());
                    foreach (var mods in Modules)
                    {
                        currentmod = mods.Name;
                        await UnloadModule(mods);
                    }
                }
                catch (Exception ex)
                {
                    Bot.Logs.Add(new Models.LogMessage("ModH-UA", $"Module {currentmod}.lmod failed to uninstall", LogSeverity.Error, ex));
                }
            }
            else
                return;
        }
        public static Task<bool> CheckDir()
        {
            if (!Directory.Exists(CurrentDir))
            {
                Bot.Logs.Add(new Models.LogMessage("ModH-CD", "No modules found, ignoring...", LogSeverity.Warning));
                Directory.CreateDirectory(CurrentDir);
                return Task.FromResult(false);
            }
            else if(Directory.GetFiles(CurrentDir).Length == 0)
            {
                return Task.FromResult(false);
            }
            else
            {
                return Task.FromResult(true);
            }
        }

        public static async Task<bool> LoadSpecificModule(string module)
        {
            if (Char.IsLower(module[0]))
                module = module.First().ToString().ToUpper() + String.Join("", module.Skip(1));
            if (module.EndsWith(".dll"))
                module = module.Substring(0, module.Length - 4);
            if (await CheckDir())
            {
                foreach (string mod in Directory.GetFiles(CurrentDir))
                {
                    string newMod = mod.Split('\\')[mod.Split('\\').Count() - 1];
                    newMod = newMod.Substring(0, newMod.Length - 4);
                    if (newMod == module)
                    {
                        if (!mod.EndsWith(".dll")) continue;
                        await LoadModule(mod);
                        return true;
                    }
                }
                return false;
            }
            else
                return false;
        }
        public static async Task ReloadSpecific(string module)
        {
            if (await CheckDir())
            {
                await UnloadSpecificModule(module);
                await Task.Delay(250);
                await LoadSpecificModule(module);
            }
            else
                return;
        }
        public static async Task<bool> UnloadSpecificModule(string module)
        {
            if (Char.IsLower(module[0]))
                module = module.First().ToString().ToUpper() + String.Join("", module.Skip(1));
            if (module.EndsWith(".dll"))
                module = module.Substring(0, module.Length - 4);
            if (await CheckDir())
            {
                try
                {
                    foreach (var mods in Bot.commands.Modules)
                        if (mods.Name == module)
                            if(await UnloadModule(mods)) return true;
                }
                catch (Exception ex)
                {
                    Bot.Logs.Add(new Models.LogMessage("ModH-US", $"Module {module}.lmod failed to uninstall", LogSeverity.Error, ex));
                    return false;
                }
                return true;
            }
            else
                return false;
        }

        private static async Task<bool> LoadModule(string mod)
        {
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(mod);
            Assembly assemblymod = Assembly.Load(File.ReadAllBytes(mod));
            foreach (var module in Bot.commands.Modules)
            {
                if (module.Name == assemblymod.GetName().Name) return false;
            }
            if (assemblymod.GetName().Version < Assembly.GetExecutingAssembly().GetName().Version) throw new Exceptions.IncorrectVersionException($"{assemblymod.GetName().Name}.lmod will not work with this version of Skuld!");
            try
            {
                await Bot.commands.AddModulesAsync(assemblymod);
                foreach (var module in Bot.commands.Modules)
                    if (module.Name == assemblymod.GetName().Name)
                    {
                        Bot.Logs.Add(new Models.LogMessage("ModH-LM", $"Loaded: {assemblymod.GetName().Name}.lmod", LogSeverity.Info));
                        return true;
                    }
            }
            catch (Exception ex)
            {
                var typeLoadException = ex as ReflectionTypeLoadException;
                var loaderException = typeLoadException.LoaderExceptions;
                Bot.Logs.Add(new Models.LogMessage("ModH-LS", $"Module {assemblymod.GetName().Name}.lmod failed to install", LogSeverity.Error, loaderException.FirstOrDefault()));
                return false;
            }
            return false;
        }
        private static async Task<bool> UnloadModule(Discord.Commands.ModuleInfo mod)
        {
            await Bot.commands.RemoveModuleAsync(mod).ContinueWith(x =>
            {
                if(!Bot.commands.Modules.Contains(mod))
                    Bot.Logs.Add(new Models.LogMessage("ModH-UM", $"Removed: {mod.Name}.lmod", LogSeverity.Info));
            });
            if (Bot.commands.Modules.Contains(mod)) return false;
            else return true;
        }

        public static async Task CountModulesCommands()
        {
            Bot.Logs.Add(new Models.LogMessage("CmdSrvc", $"Loaded {Bot.commands.Commands.Count()} Commands from {Bot.commands.Modules.Count()} Modules", LogSeverity.Info));
            await Task.Delay(1);
        }
    }
}
