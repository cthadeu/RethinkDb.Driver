﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using RethinkDb.Driver.Model;
using RethinkDb.Driver.Net;
using RethinkDb.Driver.Tests.Utils;

namespace RethinkDb.Driver.Tests.ReQL
{
    public class Foo
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string id { get; set; }

        public int Bar { get; set; }
        public int Baz { get; set; }
        public string Idx { get; set; }
        public DateTimeOffset? Tim { get; set; }
    }

    [TestFixture]
    public class Examples : QueryTestFixture
    {
        [Test]
        public void test_booleans()
        {
            bool t = R.expr(true).run<bool>(conn);
            t.Should().Be(true);
        }

        [Test]
        public void insert_test_without_id()
        {
            var obj = new Foo { Bar = 1, Baz = 2, Tim = DateTimeOffset.Now };
            Result result = R.db(DbName).table(TableName).insert(obj).run<Result>(conn);
            result.Dump();
        }

        [Test]
        public void insert_an_array_of_pocos()
        {
            var arr = new[]
                {
                    new Foo {id = "a", Baz = 1, Bar = 1},
                    new Foo {id = "b", Baz = 2, Bar = 2},
                    new Foo {id = "c", Baz = 3, Bar = 3}
                };
            Result result = R.db(DbName).table(TableName).insert(arr).run<Result>(conn);
            result.Dump();
        }

        [Test]
        public void get_test()
        {
            Foo foo = R.db(DbName).table(TableName).get("a").run<Foo>(conn);
            foo.Dump();
        }

        [Test]
        public void get_with_time()
        {
            Foo foo = R.db(DbName).table(TableName).get("4d4ba69e-048c-43b7-b842-c7b49dc6691c")
                .run<Foo>(conn);

            foo.Dump();
        }

        [Test]
        public void getall_test()
        {
            Cursor<Foo> all = R.db(DbName).table(TableName).getAll("a", "b", "c").run<Foo>(conn);

            all.BufferedItems.Dump();

            foreach (var foo in all)
            {
                Console.WriteLine($"Printing: {foo.id}!");
                foo.Dump();
            }
        }

        [Test]
        public void use_a_cursor_to_get_items()
        {
            Cursor<Foo> all = R.db(DbName).table(TableName).getAll("a", "b", "c").runCursor<Foo>(conn);

            foreach (var foo in all)
            {
                Console.WriteLine($"Printing: {foo.id}!");
                foo.Dump();
            }
        }

        [Test]
        public void getall_with_linq()
        {
            Cursor<Foo> all = R.db(DbName).table(TableName).getAll("a", "b", "c").runCursor<Foo>(conn);

            var bazInOrder = all.OrderByDescending(f => f.Baz)
                .Select(f => f.Baz);
            foreach (var baz in bazInOrder)
            {
                Console.WriteLine(baz);
            }
        }

        [Test]
        public void getall_using_an_index_with_optarg_indexer()
        {
            const string IndexName = "Idx";

            DropTable(DbName, TableName);
            CreateTable(DbName, TableName);

            R.db(DbName)
                .table(TableName)
                .indexCreate(IndexName).run(conn);

            R.db(DbName)
                .table(TableName)
                .indexWait(IndexName).run(conn);

            var foos = new[]
                {
                    new Foo {id = "a", Baz = 1, Bar = 1, Idx = "qux"},
                    new Foo {id = "b", Baz = 2, Bar = 2, Idx = "bub"},
                    new Foo {id = "c", Baz = 3, Bar = 3, Idx = "qux"}
                };

            R.db(DbName).table(TableName).insert(foos).run(conn);

            Cursor<Foo> all = R.db(DbName).table(TableName)
                .getAll("qux")[new { index = "Idx" }]
                .run<Foo>(conn);

            var results = all.ToArray();

            var onlyQux = foos.Where(f => f.Idx == "qux");

            results.ShouldAllBeEquivalentTo(onlyQux);
        }

        [Test]
        public void getfield_expression_test()
        {
            R.db(DbName).table(TableName).delete().run(conn);
            var arr = new[]
                {
                    new Foo {id = "a", Baz = 1, Bar = 1, Tim = DateTimeOffset.Now},
                    new Foo {id = "b", Baz = 2, Bar = 2, Tim = DateTimeOffset.Now},
                    new Foo {id = "c", Baz = 3, Bar = 3, Tim = DateTimeOffset.Now}
                };
            Result result = R.db(DbName).table(TableName).insert(arr).run<Result>(conn);
            result.Dump();
            result.Inserted.Should().Be(3);

            long bazInFooC = R.db(DbName).table(TableName).get("c")["Baz"].run(conn);
            bazInFooC.Should().Be(3);
        }

        [Test]
        public void test_overloading()
        {
            R.db(DbName).table(TableName).delete().run(conn);
            var arr = new[]
                {
                    new Foo {id = "a", Baz = 1, Bar = 1, Tim = DateTimeOffset.Now},
                    new Foo {id = "b", Baz = 2, Bar = 2, Tim = DateTimeOffset.Now},
                    new Foo {id = "c", Baz = 3, Bar = 3, Tim = DateTimeOffset.Now}
                };
            Result result = R.db(DbName).table(TableName).insert(arr).run<Result>(conn);
            result.Dump();
            result.Inserted.Should().Be(3);

            var expA = R.db(DbName).table(TableName).get("a")["Baz"];
            var expB = R.db(DbName).table(TableName).get("b")["Bar"];

            int add = (expA + expB + 1).run<int>(conn);
            add.Should().Be(4);
        }

        [Test]
        public void test_implicit_operator_overload()
        {
            long x = (R.expr(1) + 1).run(conn); //everything between () actually gets executed on the server
            x.Should().Be(2);
        }

        [Test]
        public void test_loop()
        {
            Cursor<int> result = R.range(1, 4).runCursor<int>(conn);

            foreach (var i in result)
            {
                Console.WriteLine(i);
            }
        }

        public class Avatar
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            public byte[] ImageData { get; set; }
        }

        [Test]
        public void insert_some_binary_data()
        {
            var data = Enumerable.Range(0, 256)
                .Select(Convert.ToByte)
                .ToArray();

            var avatar = new Avatar
                {
                    Id = "myavatar",
                    ImageData = data,
                };


            R.db(DbName).table(TableName).delete().run(conn);

            R.db(DbName).table(TableName)
                .insert(avatar).run(conn);


            Avatar fromDb = R.db(DbName).table(TableName)
                .get("myavatar").run<Avatar>(conn);


            fromDb.Id.Should().Be(avatar.Id);
            fromDb.ImageData.Should().Equal(data);
        }

        [Test]
        public void insert_some_binary_the_java_way()
        {
            var data = Enumerable.Range(0, 256)
                .Select(Convert.ToByte)
                .ToArray();

            var myObject = new MapObject()
                {
                    {"id", "javabin"},
                    {"the_data", R.binary(data)}
                };

            R.db(DbName).table(TableName)
                .insert(myObject).run(conn);

            var result = R.db(DbName).table(TableName)
                .get("javabin").run(conn);

            ExtensionsForTesting.Dump(result.the_data);
        }

        [Test]
        public void server_info()
        {
            var server = conn.server();

            server.Id.Should().NotBeEmpty();
            server.Name.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void check_if_table_exists()
        {
            var result = R.db(DbName).tableList().runAtom<List<string>>(conn);

            if( result.Contains("test") )
            {
                //exists
            }
            else
            {
                //doesnt exist
            }

            var newTableResult = R.db(DbName).tableList().contains("newTable")
                .do_(tableExists =>
                    {
                        return R.branch(
                            tableExists, /* The test */
                            new {tables_created = 0}, /* If False */
                            R.db(DbName).tableCreate("newTable") /* If true */
                            );
                    }).runResult(conn);

            newTableResult.Dump();
        }

    }


}