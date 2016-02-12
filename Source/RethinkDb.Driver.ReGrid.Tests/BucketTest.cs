using System.Threading;
using NUnit.Framework;
using RethinkDb.Driver.Ast;
using RethinkDb.Driver.Model;
using RethinkDb.Driver.Tests;

namespace RethinkDb.Driver.ReGrid.Tests
{
    public class BucketTest : QueryTestFixture
    {
        protected Table fileTable;
        protected Table chunkTable;
        protected Db db;
        protected string chunkIndex;
        protected string fileIndexPath;
        protected Bucket bucket;
        protected string fileTableName;
        protected string chunkTableName;

        protected string testfile = "foobar.mp3";

        public BucketTest()
        {
            Log.TruncateBinaryTypes = true;

            fileTable = R.db(DbName).table("fs_files");
            chunkTable = R.db(DbName).table("fs_chunk");
            db = R.db(DbName);
            var opts = new BucketConfig();
            chunkIndex = opts.ChunkIndex;
            fileIndexPath = opts.FileIndex;
            fileTableName = "fs_files";
            chunkTableName = "fs_chunks";
        }

        [SetUp]
        public void BeforeEachTest()
        {
            //make sure we get a new conn on each test.
            bucket = new Bucket(conn, DbName);
        }

        protected void DropFilesTable()
        {
            var result = db.tableDrop(this.fileTableName).runResult(this.conn);
            result.AssertTablesDropped(1);
        }

        public void ClearBucket()
        {
            bucket.Purge();
            bucket.Mount();
        }


        protected void CreateBucketWithTwoFileRevisions()
        {
            ClearBucket();

            //original reversed
            bucket.Upload(testfile, TestBytes.OneHalfChunk);

            Thread.Sleep(1500);

            //latest
            bucket.Upload(testfile, TestBytes.OneHalfChunkReversed);
        }

        protected void CreateBucketWithOneFileTwoChunks()
        {
            ClearBucket();

            //original reversed
            bucket.Upload(testfile, TestBytes.OneHalfChunk);
        }
    }
}