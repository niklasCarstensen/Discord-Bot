﻿using Discord;

namespace MEE7.Backend
{
    public abstract class Command
    {
        public string Desc { get; private set; }
        public string CommandLine { get; private set; }
        public bool IsExperimental { get; private set; }
        public bool IsHidden { get; private set; }

        public string Prefix
        {
            get
            {
                return Program.Prefix;
            }
        }
        public string PrefixAndCommand
        {
            get
            {
                return Prefix + CommandLine;
            }
        }

        public EmbedBuilder HelpMenu;

        // May be implemented later (probably never)
        //private class SubCommand
        //{
        //    public SubCommand[] SubCommands;
        //    public string Command;
        //    public string Desc;
        //}

        public Command()
        {
            Desc = "-";
            CommandLine = this.GetType().Name;
            IsExperimental = false;
            IsHidden = true;
        }
        public Command(string command, string desc, bool isExperimental = false, bool isHidden = false)
        {
            Desc = desc;
            CommandLine = command;
            IsExperimental = isExperimental;
            IsHidden = isHidden;
        }

        public abstract void Execute(IMessage message);
    }
}
