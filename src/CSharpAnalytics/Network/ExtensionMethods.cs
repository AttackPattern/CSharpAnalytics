﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

 using System;

namespace CSharpAnalytics.Network
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Obtain the innermost Exception from within an Exception.
        /// </summary>
        /// <param name="ex">Exception to obtain the innermost exception from.</param>
        /// <returns>Innermost exception that could be obtained.</returns>
        public static Exception GetInnermostException(this Exception ex)
        {
            var nextException = ex;
            while (nextException.InnerException != null)
                nextException = nextException.InnerException;
            return nextException;
        }
    }
}