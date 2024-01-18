namespace SignalR.Protobuf.Util;

// Copied from Google.Protobuf so that I can delimit streams without variable integers.
internal sealed class LimitedInputStream : Stream
{
    private readonly Stream _proxied;
    private int _bytesLeft;

    internal LimitedInputStream(Stream proxied, int size)
    {
        _proxied = proxied;
        _bytesLeft = size;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;

    public override void Flush()
    {
    }

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_bytesLeft <= 0)
            return 0;
        var num = _proxied.Read(buffer, offset, Math.Min(_bytesLeft, count));
        _bytesLeft -= num;
        return num;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}