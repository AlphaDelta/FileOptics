﻿using FileOptics.Interface;
using System;
using System.Collections.Generic;
using System.Text;

internal class Root
{
    internal static List<byte[]> TrustedModules = new List<byte[]>();
    internal static List<IModule> Modules = new List<IModule>();
    internal static List<ModuleAttrib> ModuleAttribs = new List<ModuleAttrib>();
}

public delegate void Action();