﻿namespace PeregrineDb.Databases.Mapper
{
    using System;
    using System.Data;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;

    /// <summary>
    /// Represents the key aspects of a sql operation
    /// </summary>
    internal struct CommandDefinition
    {
        internal void OnCompleted()
        {
            (this.Parameters as IParameterCallbacks)?.OnCompleted();
        }

        /// <summary>
        /// The command (sql or a stored-procedure name) to execute
        /// </summary>
        public string CommandText { get; }

        /// <summary>
        /// The parameters associated with the command
        /// </summary>
        public object Parameters { get; }

        /// <summary>
        /// The active transaction for the command
        /// </summary>
        public IDbTransaction Transaction { get; }

        /// <summary>
        /// The effective timeout for the command
        /// </summary>
        public int? CommandTimeout { get; }

        /// <summary>
        /// The type of command that the command-text represents
        /// </summary>
        public CommandType? CommandType { get; }

        /// <summary>
        /// Should data be buffered before returning?
        /// </summary>
        public bool Buffered => (this.Flags & CommandFlags.Buffered) != 0;

        /// <summary>
        /// Should the plan for this query be cached?
        /// </summary>
        internal bool AddToCache => (this.Flags & CommandFlags.NoCache) == 0;

        /// <summary>
        /// Additional state flags against this command
        /// </summary>
        public CommandFlags Flags { get; }

        /// <summary>
        /// Initialize the command definition
        /// </summary>
        /// <param name="commandText">The text for this command.</param>
        /// <param name="parameters">The parameters for this command.</param>
        /// <param name="transaction">The transaction for this command to participate in.</param>
        /// <param name="commandTimeout">The timeout (in seconds) for this command.</param>
        /// <param name="commandType">The <see cref="CommandType"/> for this command.</param>
        /// <param name="flags">The behavior flags for this command.</param>
        /// <param name="cancellationToken">The cancellation token for this command.</param>
        public CommandDefinition(
            string commandText,
            object parameters = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null,
            CommandFlags flags = CommandFlags.Buffered,
            CancellationToken cancellationToken = default)
        {
            this.CommandText = commandText;
            this.Parameters = parameters;
            this.Transaction = transaction;
            this.CommandTimeout = commandTimeout;
            this.CommandType = commandType;
            this.Flags = flags;
            this.CancellationToken = cancellationToken;
        }

        /// <summary>
        /// For asynchronous operations, the cancellation-token
        /// </summary>
        public CancellationToken CancellationToken { get; }

        internal IDbCommand SetupCommand(IDbConnection cnn, Action<IDbCommand, object> paramReader)
        {
            var cmd = cnn.CreateCommand();
            var init = GetInit(cmd.GetType());
            init?.Invoke(cmd);
            if (this.Transaction != null)
                cmd.Transaction = this.Transaction;
            cmd.CommandText = this.CommandText;
            if (this.CommandTimeout.HasValue)
            {
                cmd.CommandTimeout = this.CommandTimeout.Value;
            }
            else if (MapperSettings.CommandTimeout.HasValue)
            {
                cmd.CommandTimeout = MapperSettings.CommandTimeout.Value;
            }
            if (this.CommandType.HasValue)
                cmd.CommandType = this.CommandType.Value;
            paramReader?.Invoke(cmd, this.Parameters);
            return cmd;
        }

        private static Link<Type, Action<IDbCommand>> commandInitCache;

        private static Action<IDbCommand> GetInit(Type commandType)
        {
            if (commandType == null)
                return null; // GIGO
            if (Link<Type, Action<IDbCommand>>.TryGet(commandInitCache, commandType, out Action<IDbCommand> action))
            {
                return action;
            }
            var bindByName = GetBasicPropertySetter(commandType, "BindByName", typeof(bool));
            var initialLongFetchSize = GetBasicPropertySetter(commandType, "InitialLONGFetchSize", typeof(int));

            action = null;
            if (bindByName != null || initialLongFetchSize != null)
            {
                var method = new DynamicMethod(commandType.Name + "_init", null, new Type[] { typeof(IDbCommand) });
                var il = method.GetILGenerator();

                if (bindByName != null)
                {
                    // .BindByName = true
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, commandType);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.EmitCall(OpCodes.Callvirt, bindByName, null);
                }
                if (initialLongFetchSize != null)
                {
                    // .InitialLONGFetchSize = -1
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, commandType);
                    il.Emit(OpCodes.Ldc_I4_M1);
                    il.EmitCall(OpCodes.Callvirt, initialLongFetchSize, null);
                }
                il.Emit(OpCodes.Ret);
                action = (Action<IDbCommand>)method.CreateDelegate(typeof(Action<IDbCommand>));
            }
            // cache it
            Link<Type, Action<IDbCommand>>.TryAdd(ref commandInitCache, commandType, ref action);
            return action;
        }

        private static MethodInfo GetBasicPropertySetter(Type declaringType, string name, Type expectedType)
        {
            var prop = declaringType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop?.CanWrite == true && prop.PropertyType == expectedType && prop.GetIndexParameters().Length == 0)
            {
                return prop.GetSetMethod();
            }
            return null;
        }
    }
}
