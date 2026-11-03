/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
 * Modified in 2026 by Naman for SAM Modern
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */
using System;

namespace SAM.API
{
    public class ClientInitializeException : Exception
    {
        public readonly ClientInitializeFailure Failure;

        public string UserMessage => this.Failure switch
        {
            ClientInitializeFailure.GetInstallPath =>
                "Steam doesn't appear to be installed. Please install Steam then run this tool again.",
            ClientInitializeFailure.Load =>
                "Steam's client library could not be loaded. Please restart Steam then run this tool again.",
            ClientInitializeFailure.CreateSteamClient =>
                "This version of Steam isn't supported. Please check for a newer version of this tool.",
            ClientInitializeFailure.CreateSteamPipe =>
                "Could not connect to Steam. Please make sure Steam is running, and that Steam and this tool\n" +
                "are running the same way (both normally, or both as administrator).",
            ClientInitializeFailure.ConnectToGlobalUser =>
                "Could not connect to your Steam account. Please make sure you are logged in to Steam.",
            ClientInitializeFailure.AppIdMismatch =>
                "Steam reported a different game than the one requested. Please restart Steam then try again.",
            _ => "Steam is not running. Please start Steam then run this tool again.",
        };

        public ClientInitializeException(ClientInitializeFailure failure)
        {
            this.Failure = failure;
        }

        public ClientInitializeException(ClientInitializeFailure failure, string message)
            : base(message)
        {
            this.Failure = failure;
        }

        public ClientInitializeException(ClientInitializeFailure failure, string message, Exception innerException)
            : base(message, innerException)
        {
            this.Failure = failure;
        }
    }
}
