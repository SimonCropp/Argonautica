// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

namespace Argon;

public partial class JsonTextWriter
{
    // It's not safe to perform the async methods here in a derived class as if the synchronous equivalent
    // has been overriden then the asychronous method will no longer be doing the same operation.
    readonly bool safeAsync;

    /// <summary>
    /// Asynchronously flushes whatever is in the buffer to the destination and also flushes the destination.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task FlushAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoFlushAsync(cancellation) : base.FlushAsync(cancellation);
    }

    internal Task DoFlushAsync(CancellationToken cancellation)
    {
        return cancellation.CancelIfRequestedAsync() ?? writer.FlushAsync();
    }

    /// <summary>
    /// Asynchronously writes the JSON value delimiter.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    protected override Task WriteValueDelimiterAsync(CancellationToken cancellation)
    {
        return safeAsync ? DoWriteValueDelimiterAsync(cancellation) : base.WriteValueDelimiterAsync(cancellation);
    }

    internal Task DoWriteValueDelimiterAsync(CancellationToken cancellation)
    {
        return writer.WriteAsync(',', cancellation);
    }

    /// <summary>
    /// Asynchronously writes the specified end token.
    /// </summary>
    /// <param name="token">The end token to write.</param>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    protected override Task WriteEndAsync(JsonToken token, CancellationToken cancellation)
    {
        return safeAsync ? DoWriteEndAsync(token, cancellation) : base.WriteEndAsync(token, cancellation);
    }

    internal Task DoWriteEndAsync(JsonToken token, CancellationToken cancellation)
    {
        switch (token)
        {
            case JsonToken.EndObject:
                return writer.WriteAsync('}', cancellation);
            case JsonToken.EndArray:
                return writer.WriteAsync(']', cancellation);
            default:
                throw JsonWriterException.Create(this, $"Invalid JsonToken: {token}", null);
        }
    }

    /// <summary>
    /// Asynchronously closes this writer.
    /// If <see cref="JsonWriter.CloseOutput"/> is set to <c>true</c>, the destination is also closed.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task CloseAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoCloseAsync(cancellation) : base.CloseAsync(cancellation);
    }

    internal async Task DoCloseAsync(CancellationToken cancellation)
    {
        if (Top == 0) // otherwise will happen in calls to WriteEndAsync
        {
            cancellation.ThrowIfCancellationRequested();
        }

        while (Top > 0)
        {
            await WriteEndAsync(cancellation).ConfigureAwait(false);
        }

        CloseBufferAndWriter();
    }

    /// <summary>
    /// Asynchronously writes the end of the current JSON object or array.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteEndAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? WriteEndInternalAsync(cancellation) : base.WriteEndAsync(cancellation);
    }

    /// <summary>
    /// Asynchronously writes indent characters.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    protected override Task WriteIndentAsync(CancellationToken cancellation)
    {
        return safeAsync ? DoWriteIndentAsync(cancellation) : base.WriteIndentAsync(cancellation);
    }

    internal Task DoWriteIndentAsync(CancellationToken cancellation)
    {
        // levels of indentation multiplied by the indent count
        var currentIndentCount = Top * indentation;

        var newLineLen = SetIndentChars();
        MiscellaneousUtils.Assert(indentChars != null);

        if (currentIndentCount <= indentCharBufferSize)
        {
            return writer.WriteAsync(indentChars, 0, newLineLen + currentIndentCount, cancellation);
        }

        return WriteIndentAsync(currentIndentCount, newLineLen, cancellation);
    }

    async Task WriteIndentAsync(int currentIndentCount, int newLineLen, CancellationToken cancellation)
    {
        MiscellaneousUtils.Assert(indentChars != null);

        await writer.WriteAsync(indentChars, 0, newLineLen + Math.Min(currentIndentCount, indentCharBufferSize), cancellation).ConfigureAwait(false);

        while ((currentIndentCount -= indentCharBufferSize) > 0)
        {
            await writer.WriteAsync(indentChars, newLineLen, Math.Min(currentIndentCount, indentCharBufferSize), cancellation).ConfigureAwait(false);
        }
    }

    Task WriteValueInternalAsync(JsonToken token, string value, CancellationToken cancellation)
    {
        var task = InternalWriteValueAsync(token, cancellation);
        if (task.IsCompletedSucessfully())
        {
            return writer.WriteAsync(value, cancellation);
        }

        return WriteValueInternalAsync(task, value, cancellation);
    }

    async Task WriteValueInternalAsync(Task task, string value, CancellationToken cancellation)
    {
        await task.ConfigureAwait(false);
        await writer.WriteAsync(value, cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes an indent space.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    protected override Task WriteIndentSpaceAsync(CancellationToken cancellation)
    {
        if (safeAsync)
        {
            return DoWriteIndentSpaceAsync(cancellation);
        }

        return base.WriteIndentSpaceAsync(cancellation);
    }

    internal Task DoWriteIndentSpaceAsync(CancellationToken cancellation)
    {
        return writer.WriteAsync(' ', cancellation);
    }

    /// <summary>
    /// Asynchronously writes raw JSON without changing the writer's state.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteRawAsync(string? json, CancellationToken cancellation = default)
    {
        if (safeAsync)
        {
            return DoWriteRawAsync(json, cancellation);
        }

        return base.WriteRawAsync(json, cancellation);
    }

    internal Task DoWriteRawAsync(string? json, CancellationToken cancellation)
    {
        return writer.WriteAsync(json, cancellation);
    }

    /// <summary>
    /// Asynchronously writes a null value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteNullAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteNullAsync(cancellation) : base.WriteNullAsync(cancellation);
    }

    internal Task DoWriteNullAsync(CancellationToken cancellation)
    {
        return WriteValueInternalAsync(JsonToken.Null, JsonConvert.Null, cancellation);
    }

    Task WriteDigitsAsync(ulong uvalue, bool negative, CancellationToken cancellation)
    {
        if (uvalue <= 9 & !negative)
        {
            return writer.WriteAsync((char)('0' + uvalue), cancellation);
        }

        var length = WriteNumberToBuffer(uvalue, negative);
        return writer.WriteAsync(writeBuffer!, 0, length, cancellation);
    }

    Task WriteIntegerValueAsync(ulong uvalue, bool negative, CancellationToken cancellation)
    {
        var task = InternalWriteValueAsync(JsonToken.Integer, cancellation);
        if (task.IsCompletedSucessfully())
        {
            return WriteDigitsAsync(uvalue, negative, cancellation);
        }

        return WriteIntegerValueAsync(task, uvalue, negative, cancellation);
    }

    async Task WriteIntegerValueAsync(Task task, ulong uvalue, bool negative, CancellationToken cancellation)
    {
        await task.ConfigureAwait(false);
        await WriteDigitsAsync(uvalue, negative, cancellation).ConfigureAwait(false);
    }

    internal Task WriteIntegerValueAsync(long value, CancellationToken cancellation)
    {
        var negative = value < 0;
        if (negative)
        {
            value = -value;
        }

        return WriteIntegerValueAsync((ulong)value, negative, cancellation);
    }

    internal Task WriteIntegerValueAsync(ulong uvalue, CancellationToken cancellation)
    {
        return WriteIntegerValueAsync(uvalue, false, cancellation);
    }

    Task WriteEscapedStringAsync(string value, bool quote, CancellationToken cancellation)
    {
        return JavaScriptUtils.WriteEscapedJavaScriptStringAsync(writer, value, quoteChar, quote, charEscapeFlags!, StringEscapeHandling, this, writeBuffer!, cancellation);
    }

    /// <summary>
    /// Asynchronously writes the property name of a name/value pair of a JSON object.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WritePropertyNameAsync(string name, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWritePropertyNameAsync(name, cancellation) : base.WritePropertyNameAsync(name, cancellation);
    }

    internal Task DoWritePropertyNameAsync(string name, CancellationToken cancellation)
    {
        var task = InternalWritePropertyNameAsync(name, cancellation);
        if (!task.IsCompletedSucessfully())
        {
            return DoWritePropertyNameAsync(task, name, cancellation);
        }

        task = WriteEscapedStringAsync(name, QuoteName, cancellation);
        if (task.IsCompletedSucessfully())
        {
            return writer.WriteAsync(':', cancellation);
        }

        return JavaScriptUtils.WriteCharAsync(task, writer, ':', cancellation);
    }

    async Task DoWritePropertyNameAsync(Task task, string name, CancellationToken cancellation)
    {
        await task.ConfigureAwait(false);

        await WriteEscapedStringAsync(name, QuoteName, cancellation).ConfigureAwait(false);

        await writer.WriteAsync(':').ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes the property name of a name/value pair of a JSON object.
    /// </summary>
    /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WritePropertyNameAsync(string name, bool escape, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWritePropertyNameAsync(name, escape, cancellation) : base.WritePropertyNameAsync(name, escape, cancellation);
    }

    internal async Task DoWritePropertyNameAsync(string name, bool escape, CancellationToken cancellation)
    {
        await InternalWritePropertyNameAsync(name, cancellation).ConfigureAwait(false);

        if (escape)
        {
            await WriteEscapedStringAsync(name, QuoteName, cancellation).ConfigureAwait(false);
        }
        else
        {
            if (QuoteName)
            {
                await writer.WriteAsync(quoteChar).ConfigureAwait(false);
            }

            await writer.WriteAsync(name, cancellation).ConfigureAwait(false);

            if (QuoteName)
            {
                await writer.WriteAsync(quoteChar).ConfigureAwait(false);
            }
        }

        await writer.WriteAsync(':').ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes the beginning of a JSON array.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteStartArrayAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteStartArrayAsync(cancellation) : base.WriteStartArrayAsync(cancellation);
    }

    internal Task DoWriteStartArrayAsync(CancellationToken cancellation)
    {
        var task = InternalWriteStartAsync(JsonToken.StartArray, JsonContainerType.Array, cancellation);
        if (task.IsCompletedSucessfully())
        {
            return writer.WriteAsync('[', cancellation);
        }

        return DoWriteStartArrayAsync(task, cancellation);
    }

    internal async Task DoWriteStartArrayAsync(Task task, CancellationToken cancellation)
    {
        await task.ConfigureAwait(false);

        await writer.WriteAsync('[', cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes the beginning of a JSON object.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteStartObjectAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteStartObjectAsync(cancellation) : base.WriteStartObjectAsync(cancellation);
    }

    internal Task DoWriteStartObjectAsync(CancellationToken cancellation)
    {
        var task = InternalWriteStartAsync(JsonToken.StartObject, JsonContainerType.Object, cancellation);
        if (task.IsCompletedSucessfully())
        {
            return writer.WriteAsync('{', cancellation);
        }

        return DoWriteStartObjectAsync(task, cancellation);
    }

    internal async Task DoWriteStartObjectAsync(Task task, CancellationToken cancellation)
    {
        await task.ConfigureAwait(false);

        await writer.WriteAsync('{', cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes an undefined value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteUndefinedAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteUndefinedAsync(cancellation) : base.WriteUndefinedAsync(cancellation);
    }

    internal Task DoWriteUndefinedAsync(CancellationToken cancellation)
    {
        var task = InternalWriteValueAsync(JsonToken.Undefined, cancellation);
        if (task.IsCompletedSucessfully())
        {
            return writer.WriteAsync(JsonConvert.Undefined, cancellation);
        }

        return DoWriteUndefinedAsync(task, cancellation);
    }

    async Task DoWriteUndefinedAsync(Task task, CancellationToken cancellation)
    {
        await task.ConfigureAwait(false);
        await writer.WriteAsync(JsonConvert.Undefined, cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes the given white space.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteWhitespaceAsync(string ws, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteWhitespaceAsync(ws, cancellation) : base.WriteWhitespaceAsync(ws, cancellation);
    }

    internal Task DoWriteWhitespaceAsync(string ws, CancellationToken cancellation)
    {
        InternalWriteWhitespace(ws);
        return writer.WriteAsync(ws, cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="bool"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(bool value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(bool value, CancellationToken cancellation)
    {
        return WriteValueInternalAsync(JsonToken.Boolean, JsonConvert.ToString(value), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="bool"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(bool? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(bool? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : DoWriteValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="byte"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(byte value, CancellationToken cancellation = default)
    {
        return safeAsync ? WriteIntegerValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="byte"/> value.
    /// </summary>
    /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="byte"/> value to write.</param>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(byte? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(byte? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="byte"/>[] value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(byte[]? value, CancellationToken cancellation = default)
    {
        return safeAsync ? value == null ? WriteNullAsync(cancellation) : WriteValueNonNullAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal async Task WriteValueNonNullAsync(byte[] value, CancellationToken cancellation)
    {
        await InternalWriteValueAsync(JsonToken.Bytes, cancellation).ConfigureAwait(false);
        await writer.WriteAsync(quoteChar).ConfigureAwait(false);
        await Base64Encoder.EncodeAsync(value, 0, value.Length, cancellation).ConfigureAwait(false);
        await Base64Encoder.FlushAsync(cancellation).ConfigureAwait(false);
        await writer.WriteAsync(quoteChar).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="char"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(char value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(char value, CancellationToken cancellation)
    {
        return WriteValueInternalAsync(JsonToken.String, JsonConvert.ToString(value), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="char"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(char? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(char? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : DoWriteValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="DateTime"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(DateTime value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal async Task DoWriteValueAsync(DateTime value, CancellationToken cancellation)
    {
        await InternalWriteValueAsync(JsonToken.Date, cancellation).ConfigureAwait(false);
        value = DateTimeUtils.EnsureDateTime(value, DateTimeZoneHandling);

        if (StringUtils.IsNullOrEmpty(DateFormatString))
        {
            var length = WriteValueToBuffer(value);

            await writer.WriteAsync(writeBuffer!, 0, length, cancellation).ConfigureAwait(false);
        }
        else
        {
            await writer.WriteAsync(quoteChar).ConfigureAwait(false);
            await writer.WriteAsync(value.ToString(DateFormatString, Culture), cancellation).ConfigureAwait(false);
            await writer.WriteAsync(quoteChar).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="DateTime"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(DateTime? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(DateTime? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : DoWriteValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="DateTimeOffset"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(DateTimeOffset value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal async Task DoWriteValueAsync(DateTimeOffset value, CancellationToken cancellation)
    {
        await InternalWriteValueAsync(JsonToken.Date, cancellation).ConfigureAwait(false);

        if (StringUtils.IsNullOrEmpty(DateFormatString))
        {
            var length = WriteValueToBuffer(value);

            await writer.WriteAsync(writeBuffer!, 0, length, cancellation).ConfigureAwait(false);
        }
        else
        {
            await writer.WriteAsync(quoteChar).ConfigureAwait(false);
            await writer.WriteAsync(value.ToString(DateFormatString, Culture), cancellation).ConfigureAwait(false);
            await writer.WriteAsync(quoteChar).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(DateTimeOffset? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(DateTimeOffset? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : DoWriteValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="decimal"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(decimal value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(decimal value, CancellationToken cancellation)
    {
        return WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="decimal"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(decimal? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(decimal? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : DoWriteValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="double"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(double value, CancellationToken cancellation = default)
    {
        return safeAsync ? WriteValueAsync(value, false, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task WriteValueAsync(double value, bool nullable, CancellationToken cancellation)
    {
        return WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, nullable), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="double"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(double? value, CancellationToken cancellation = default)
    {
        return safeAsync ? value.HasValue ? WriteValueAsync(value.GetValueOrDefault(), true, cancellation) : WriteNullAsync(cancellation) : base.WriteValueAsync(value, cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="float"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(float value, CancellationToken cancellation = default)
    {
        return safeAsync ? WriteValueAsync(value, false, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task WriteValueAsync(float value, bool nullable, CancellationToken cancellation)
    {
        return WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value, FloatFormatHandling, QuoteChar, nullable), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="float"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(float? value, CancellationToken cancellation = default)
    {
        return safeAsync ? value.HasValue ? WriteValueAsync(value.GetValueOrDefault(), true, cancellation) : WriteNullAsync(cancellation) : base.WriteValueAsync(value, cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Guid"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(Guid value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal async Task DoWriteValueAsync(Guid value, CancellationToken cancellation)
    {
        await InternalWriteValueAsync(JsonToken.String, cancellation).ConfigureAwait(false);

        await writer.WriteAsync(quoteChar).ConfigureAwait(false);
        await writer.WriteAsync(value.ToString("D", CultureInfo.InvariantCulture), cancellation).ConfigureAwait(false);
        await writer.WriteAsync(quoteChar).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="Guid"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(Guid? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(Guid? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : DoWriteValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="int"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(int value, CancellationToken cancellation = default)
    {
        return safeAsync ? WriteIntegerValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="int"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(int? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(int? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="long"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(long value, CancellationToken cancellation = default)
    {
        return safeAsync ? WriteIntegerValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="long"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(long? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(long? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellation);
    }

    internal Task WriteValueAsync(BigInteger value, CancellationToken cancellation)
    {
        return WriteValueInternalAsync(JsonToken.Integer, value.ToString(CultureInfo.InvariantCulture), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="object"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(object? value, CancellationToken cancellation = default)
    {
        if (safeAsync)
        {
            if (value == null)
            {
                return WriteNullAsync(cancellation);
            }
            if (value is BigInteger i)
            {
                return WriteValueAsync(i, cancellation);
            }

            return WriteValueAsync(this, ConvertUtils.GetTypeCode(value.GetType()), value, cancellation);
        }

        return base.WriteValueAsync(value, cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="sbyte"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(sbyte value, CancellationToken cancellation = default)
    {
        return safeAsync ? WriteIntegerValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="sbyte"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(sbyte? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(sbyte? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="short"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(short value, CancellationToken cancellation = default)
    {
        return safeAsync ? WriteIntegerValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="short"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(short? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(short? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="string"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(string? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(string? value, CancellationToken cancellation)
    {
        var task = InternalWriteValueAsync(JsonToken.String, cancellation);
        if (task.IsCompletedSucessfully())
        {
            return value == null ? writer.WriteAsync(JsonConvert.Null, cancellation) : WriteEscapedStringAsync(value, true, cancellation);
        }

        return DoWriteValueAsync(task, value, cancellation);
    }

    async Task DoWriteValueAsync(Task task, string? value, CancellationToken cancellation)
    {
        await task.ConfigureAwait(false);
        await (value == null ? writer.WriteAsync(JsonConvert.Null, cancellation) : WriteEscapedStringAsync(value, true, cancellation)).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="TimeSpan"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(TimeSpan value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal async Task DoWriteValueAsync(TimeSpan value, CancellationToken cancellation)
    {
        await InternalWriteValueAsync(JsonToken.String, cancellation).ConfigureAwait(false);
        await writer.WriteAsync(quoteChar, cancellation).ConfigureAwait(false);
        await writer.WriteAsync(value.ToString(null, CultureInfo.InvariantCulture), cancellation).ConfigureAwait(false);
        await writer.WriteAsync(quoteChar, cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="TimeSpan"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(TimeSpan? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(TimeSpan? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : DoWriteValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="uint"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(uint value, CancellationToken cancellation = default)
    {
        return safeAsync ? WriteIntegerValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="uint"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(uint? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(uint? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="ulong"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(ulong value, CancellationToken cancellation = default)
    {
        return safeAsync ? WriteIntegerValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="ulong"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(ulong? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(ulong? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Uri"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(Uri? value, CancellationToken cancellation = default)
    {
        return safeAsync ? value == null ? WriteNullAsync(cancellation) : WriteValueNotNullAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task WriteValueNotNullAsync(Uri value, CancellationToken cancellation)
    {
        var task = InternalWriteValueAsync(JsonToken.String, cancellation);
        if (task.IsCompletedSucessfully())
        {
            return WriteEscapedStringAsync(value.OriginalString, true, cancellation);
        }

        return WriteValueNotNullAsync(task, value, cancellation);
    }

    internal async Task WriteValueNotNullAsync(Task task, Uri value, CancellationToken cancellation)
    {
        await task.ConfigureAwait(false);
        await WriteEscapedStringAsync(value.OriginalString, true, cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="ushort"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(ushort value, CancellationToken cancellation = default)
    {
        return safeAsync ? WriteIntegerValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    /// <summary>
    /// Asynchronously writes a <see cref="Nullable{T}"/> of <see cref="ushort"/> value.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteValueAsync(ushort? value, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteValueAsync(value, cancellation) : base.WriteValueAsync(value, cancellation);
    }

    internal Task DoWriteValueAsync(ushort? value, CancellationToken cancellation)
    {
        return value == null ? DoWriteNullAsync(cancellation) : WriteIntegerValueAsync(value.GetValueOrDefault(), cancellation);
    }

    /// <summary>
    /// Asynchronously writes a comment <c>/*...*/</c> containing the specified text.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteCommentAsync(string? text, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteCommentAsync(text, cancellation) : base.WriteCommentAsync(text, cancellation);
    }

    internal async Task DoWriteCommentAsync(string? text, CancellationToken cancellation)
    {
        await InternalWriteCommentAsync(cancellation).ConfigureAwait(false);
        await writer.WriteAsync("/*", cancellation).ConfigureAwait(false);
        await writer.WriteAsync(text ?? string.Empty, cancellation).ConfigureAwait(false);
        await writer.WriteAsync("*/", cancellation).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes the end of an array.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteEndArrayAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? InternalWriteEndAsync(JsonContainerType.Array, cancellation) : base.WriteEndArrayAsync(cancellation);
    }

    /// <summary>
    /// Asynchronously writes the end of a JSON object.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteEndObjectAsync(CancellationToken cancellation = default)
    {
        return safeAsync ? InternalWriteEndAsync(JsonContainerType.Object, cancellation) : base.WriteEndObjectAsync(cancellation);
    }

    /// <summary>
    /// Asynchronously writes raw JSON where a value is expected and updates the writer's state.
    /// </summary>
    /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
    /// execute synchronously, returning an already-completed task.</remarks>
    public override Task WriteRawValueAsync(string? json, CancellationToken cancellation = default)
    {
        return safeAsync ? DoWriteRawValueAsync(json, cancellation) : base.WriteRawValueAsync(json, cancellation);
    }

    internal Task DoWriteRawValueAsync(string? json, CancellationToken cancellation)
    {
        UpdateScopeWithFinishedValue();
        var task = AutoCompleteAsync(JsonToken.Undefined, cancellation);
        if (task.IsCompletedSucessfully())
        {
            return WriteRawAsync(json, cancellation);
        }

        return DoWriteRawValueAsync(task, json, cancellation);
    }

    async Task DoWriteRawValueAsync(Task task, string? json, CancellationToken cancellation)
    {
        await task.ConfigureAwait(false);
        await WriteRawAsync(json, cancellation).ConfigureAwait(false);
    }

    internal char[] EnsureWriteBuffer(int length, int copyTo)
    {
        if (length < 35)
        {
            length = 35;
        }

        var buffer = writeBuffer;
        if (buffer == null)
        {
            return writeBuffer = BufferUtils.RentBuffer(arrayPool, length);
        }

        if (buffer.Length >= length)
        {
            return buffer;
        }

        var newBuffer = BufferUtils.RentBuffer(arrayPool, length);
        if (copyTo != 0)
        {
            Array.Copy(buffer, newBuffer, copyTo);
        }

        BufferUtils.ReturnBuffer(arrayPool, buffer);
        writeBuffer = newBuffer;
        return newBuffer;
    }
}