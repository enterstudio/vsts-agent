using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.Services.DistributedTask.Expressions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.VisualStudio.Services.Agent.Tests
{
    public sealed class ExpressionsL0
    {
        ////////////////////////////////////////////////////////////////////////////////
        // Type-cast rules
        ////////////////////////////////////////////////////////////////////////////////
        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void CastsToBoolean()
        {
            using (var hc = new TestHostContext(this))
            {
                Tracing trace = hc.GetTrace(nameof(CastsToBoolean));

                // Boolean
                trace.Info($"****************************************");
                trace.Info($"From Boolean");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "true"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "TRUE"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "false"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "FALSE"));

                // Number
                trace.Info($"****************************************");
                trace.Info($"From Number");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "1"));
                Assert.Equal(true, EvaluateAsBoolean(hc, ".5"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "0.5"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "2"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "-1"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "-.5"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "-0.5"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "-2"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "0"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "0.0"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "-0"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "-0.0"));

                // String
                trace.Info($"****************************************");
                trace.Info($"From String");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "'a'"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "'false'"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "'0'"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "' '"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "''"));

                // Version
                trace.Info($"****************************************");
                trace.Info($"From Version");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "1.2.3"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "1.2.3.4"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "0.0.0"));

                // Objects/Arrays
                trace.Info($"****************************************");
                trace.Info($"From Objects/Arrays");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "testData()", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new object()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "testData()", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new object[0]));
                Assert.Equal(true, EvaluateAsBoolean(hc, "testData()", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new int[0]));
                Assert.Equal(true, EvaluateAsBoolean(hc, "testData()", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new Dictionary<string, object>()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "testData()", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new JArray()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "testData()", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new JObject()));

                // Null
                trace.Info($"****************************************");
                trace.Info($"From Null");
                trace.Info($"****************************************");
                Assert.Equal(false, EvaluateAsBoolean(hc, "testData()", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, null));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void CastsToNumber()
        {
            using (var hc = new TestHostContext(this))
            {
                Tracing trace = hc.GetTrace(nameof(CastsToBoolean));

                // Boolean
                trace.Info($"****************************************");
                trace.Info($"From Boolean");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(1, true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(0, false)"));

                // Number
                trace.Info($"****************************************");
                trace.Info($"From String");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(0, '')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(123456.789, ' 123,456.789 ')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(123456.789, ' +123,456.789 ')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(-123456.789, ' -123,456.789 ')"));

                // Version
                trace.Info($"****************************************");
                trace.Info($"From Version");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2, 1.2.0)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(0, 0.0.0)"));

                // Objects/Arrays
                trace.Info($"****************************************");
                trace.Info($"From Objects/Arrays");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(0, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new object()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(0, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new object[0]));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(0, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new int[0]));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(0, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new Dictionary<string, object>()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(0, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new JArray()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(0, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new JObject()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(-1, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new object()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new object()));

                // Null
                trace.Info($"****************************************");
                trace.Info($"From Null");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('', testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, null));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void CastsToString()
        {
            using (var hc = new TestHostContext(this))
            {
                Tracing trace = hc.GetTrace(nameof(CastsToBoolean));

                // Boolean
                trace.Info($"****************************************");
                trace.Info($"From Boolean");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('true', true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('false', false)"));

                // Number
                trace.Info($"****************************************");
                trace.Info($"From Number");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('1', 1)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('0.5', .5)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('0.5', 0.5)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('2', 2)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('-1', -1)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('-0.5', -.5)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('-0.5', -0.5)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('-2', -2.0)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('0', 0)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('0', 0.0)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('0', -0)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('0', -0.0)"));

                // Version
                trace.Info($"****************************************");
                trace.Info($"From Version");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('1.2.3', 1.2.3)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('1.2.3.4', 1.2.3.4)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('0.0.0', 0.0.0)"));

                // Objects/Arrays
                trace.Info($"****************************************");
                trace.Info($"From Objects/Arrays");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne('', testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new object()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne('', testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new object[0]));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne('', testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new int[0]));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne('', testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new Dictionary<string, object>()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne('', testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new JArray()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne('', testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new JObject()));

                // Null
                trace.Info($"****************************************");
                trace.Info($"From Null");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('', testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, null));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void CastsToVersion()
        {
            using (var hc = new TestHostContext(this))
            {
                Tracing trace = hc.GetTrace(nameof(CastsToBoolean));

                trace.Info($"****************************************");
                trace.Info($"From Boolean");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(0.0.0.0, false)"));

                trace.Info($"****************************************");
                trace.Info($"From Number");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(testData(), 1.2)", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new Version(1, 2)));
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq(testData(), 1.0)", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new Version(1, 0)));

                trace.Info($"****************************************");
                trace.Info($"From String");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(1.2.3.4, '1.2.3.4')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(1.2.3, '1.2.3')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(testData(), '1.2')", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new Version(1, 2)));

                trace.Info($"****************************************");
                trace.Info($"From Objects/Arrays");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2.3, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new object()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2.3, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new object[0]));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2.3, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new int[0]));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2.3, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new Dictionary<string, object>()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2.3, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new JArray()));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2.3, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, new JObject()));

                // Null
                trace.Info($"****************************************");
                trace.Info($"From Null");
                trace.Info($"****************************************");
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2.3, testData())", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, null));
            }
        }

        private sealed class TestDataNode : FunctionNode
        {
            protected override object EvaluateCore(EvaluationContext context)
            {
                if (Parameters.Count == 0)
                {
                    return context.State;
                }

                string key = string.Join(",", Parameters.Select(x => x.EvaluateString(context)));
                var dictionary = context.State as IDictionary<string, object>;
                return dictionary[key];
            }
        }

        ////////////////////////////////////////////////////////////////////////////////
        // JObject/JArray
        ////////////////////////////////////////////////////////////////////////////////
        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void IndexesIntoComplex()
        {
            using (var hc = new TestHostContext(this))
            {
                var obj = new JObject
                {
                    {
                        "subObj",
                        new JObject
                        {
                            { "nestedProp1", "nested sub object property value 1" },
                            { "nestedProp2", "nested sub object property value 2" }
                        }
                    },
                    {
                        "prop1",
                        "property value 1"
                    },
                    {
                        "prop2",
                        "property value 2"
                    },
                    {
                        "array",
                        new JArray
                        {
                            "array element at index 0",
                            "array element at index 1",
                        }
                    }
                };

                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('property value 1', testData()['prop1'])", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, obj));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('property value 2', testData().prop2)", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, obj));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('nested sub object property value 1', testData()['subObj']['nestedProp1'])", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, obj));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('nested sub object property value 2', testData()['subObj'].nestedProp2)", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, obj));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('nested sub object property value 1', testData().subObj['nestedProp1'])", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, obj));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('nested sub object property value 2', testData().subObj.nestedProp2)", new IExtensionInfo[] { new ExtensionInfo<TestDataNode>("testData", 0, 0) }, obj));
            }
        }

        ////////////////////////////////////////////////////////////////////////////////
        // Functions
        ////////////////////////////////////////////////////////////////////////////////
        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EvaluatesAnd()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "and(true, true, true)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "and(true, true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "and(true, true, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "and(true, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "and(false, true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "and(false, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "and(true, 1)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "and(true, 0)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "and(true, 'a')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "and(true, '')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "and(true, 0.0.0.0)")); // version
                Assert.Equal(true, EvaluateAsBoolean(hc, "and(true, 1.2.3.4)"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void AndShortCircuitsAndAfterFirstFalse()
        {
            using (var hc = new TestHostContext(this))
            {
                // The gt function should never evaluate. It would would throw since 'not a number'
                // cannot be converted to a number.
                Assert.Equal(false, EvaluateAsBoolean(hc, "and(false, gt(1, 'not a number'))"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EvaluatesContains()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "contains('leading match', 'leading')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "contains('trailing match', 'match')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "contains('middle match', 'ddle mat')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "contains('case insensITIVE match', 'INSENSitive')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "contains('does not match', 'zzz')"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ContainsCastsToString()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "contains(true, true)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "contains(true, 'ru')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "contains(true, 'zzz')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "contains(true, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "contains(123456789, 456)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "contains(123456789, 321)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "contains(1.2.3.4, 1.2.3)")); // version
                Assert.Equal(true, EvaluateAsBoolean(hc, "contains(1.2.3.4, 2.3)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "contains(1.2.3.4, 3.2)"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EvaluatesEndsWith()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "endsWith('trailing match', 'match')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "endsWith('case insensITIVE', 'INSENSitive')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "endswith('leading does not match', 'leading')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "endswith('middle does not match', 'does not')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "endsWith('does not match', 'zzz')"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EndsWithCastsToString()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "endsWith(true, true)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "endsWith(true, 'ue')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "endsWith(true, 'u')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "endsWith(true, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "endsWith(123456789, 789)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "endsWith(123456789, 8)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "endsWith(1.2.3.4, 3.4)")); // version
                Assert.Equal(false, EvaluateAsBoolean(hc, "endsWith(1.2.3.4, 3)"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EvaluatesEqual()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(true, true)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(false, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq(false, true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(2, 2)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq(1, 2)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('insensITIVE', 'INSENSitive')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq('a', 'b')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(1.2.3, 1.2.3)")); // version
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq(1.2.3, 1.2.3.0)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq(1.2.3, 4.5.6)"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EqualCastsToMatchLeftSide()
        {
            using (var hc = new TestHostContext(this))
            {
                // Cast to bool.
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(true, 2)")); // number
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(false, 0)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(true, 'a')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(true, ' ')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(false, '')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(true, 1.2.3)")); // version
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(true, 0.0.0)"));

                // Cast to string.
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('TRue', true)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('FALse', false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('123456.789', 123456.789)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq('123456.000', 123456.000)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq('1.2.3', 1.2.3)")); // version

                // Cast to number (best effort).
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(1, true)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(0, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq(2, true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(123456.789, ' +123,456.7890 ')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(-123456.789, ' -123,456.7890 ')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(123000, ' 123,000.000 ')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(0, '')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq(1, 'not a number')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq(0, 'not a number')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq(1.2, 1.2.0.0)")); // version

                // Cast to version (best effort).
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq(1.2.3, false)")); // bool
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq(1.2.3, true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq(1.2.0, 1.2)")); // number
                Assert.Equal(true, EvaluateAsBoolean(hc, "eq(1.2.0, ' 1.2.0 ')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "eq(1.2.0, '1.2')"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EvaluatesGreaterThan()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "gt(true, false)")); // bool
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(true, true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(false, true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(false, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "gt(2, 1)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(2, 2)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(1, 2)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "gt('DEF', 'abc')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "gt('def', 'ABC')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt('a', 'a')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt('a', 'b')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "gt(4.5.6, 1.2.3)")); // version
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(1.2.3, 4.5.6)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(1.2.3, 1.2.3)"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void GreaterThanCastsToMatchLeftSide()
        {
            using (var hc = new TestHostContext(this))
            {
                // Cast to bool.
                Assert.Equal(true, EvaluateAsBoolean(hc, "gt(true, 0)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(true, 1)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "gt(true, '')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(true, ' ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(true, 'a')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(true, 1.2.3)")); // version
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(true, 0.0.0)"));

                // Cast to string.
                Assert.Equal(true, EvaluateAsBoolean(hc, "gt('UUU', true)")); // bool
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt('SSS', true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "gt('123456.789', 123456.78)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt('123456.789', 123456.7899)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "gt('1.2.3', 1.2.2)")); // version
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt('1.2.3', 1.2.4)"));

                // Cast to number (or fails).
                Assert.Equal(true, EvaluateAsBoolean(hc, "gt(1, false)")); // bool
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(1, true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "gt(123456.789, ' +123,456.788 ')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(123456.789, ' +123,456.7899 ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(123456.789, ' +123,456.789 ')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "gt(-123456.789, ' -123,456.7899 ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(-123456.789, ' -123,456.789 ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(-123456.789, ' -123,456.788 ')"));
                try
                {
                    EvaluateAsBoolean(hc, "gt(1, 'not a number')");
                    throw new Exception("Should not reach here.");
                }
                catch (InvalidCastException ex)
                {
                    Assert.Equal("String", GetFromKind(ex));
                    Assert.Equal("Number", GetToKind(ex));
                    Assert.Equal("not a number", GetValue(ex));
                }

                try
                {
                    EvaluateAsBoolean(hc, "gt(1.2, 1.2.0.0)"); // version
                    throw new Exception("Should not reach here.");
                }
                catch (InvalidCastException ex)
                {
                    Assert.Equal("Version", GetFromKind(ex));
                    Assert.Equal("Number", GetToKind(ex));
                    Assert.Equal(new Version("1.2.0.0"), GetValue(ex));
                }

                // Cast to version (or fails).
                try
                {
                    EvaluateAsBoolean(hc, "gt(1.2.3, false)"); // bool
                    throw new Exception("Should not reach here.");
                }
                catch (InvalidCastException ex)
                {
                    Assert.Equal("Boolean", GetFromKind(ex));
                    Assert.Equal("Version", GetToKind(ex));
                    Assert.Equal(false, GetValue(ex));
                }

                Assert.Equal(true, EvaluateAsBoolean(hc, "gt(1.2.0, 1.1)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(1.2.0, 1.3)"));
                try
                {
                    EvaluateAsBoolean(hc, "gt(1.2.0, 2147483648.1)");
                    throw new Exception("Should not reach here.");
                }
                catch (InvalidCastException ex)
                {
                    Assert.Equal("Number", GetFromKind(ex));
                    Assert.Equal("Version", GetToKind(ex));
                    Assert.Equal(2147483648.1m, GetValue(ex));
                }

                Assert.Equal(true, EvaluateAsBoolean(hc, "gt(1.2.1, ' 1.2.0 ')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(1.2.1, ' 1.2.1 ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "gt(1.2.1, ' 1.2.2 ')"));
                try
                {
                    EvaluateAsBoolean(hc, "gt(1.2.1, 'not a version')");
                    throw new Exception("Should not reach here.");
                }
                catch (InvalidCastException ex)
                {
                    Assert.Equal("String", GetFromKind(ex));
                    Assert.Equal("Version", GetToKind(ex));
                    Assert.Equal("not a version", GetValue(ex));
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EvaluatesGreaterThanOrEqual()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "ge(true, false)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "ge(true, true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "ge(false, true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ge(2, 1)")); // number
                Assert.Equal(true, EvaluateAsBoolean(hc, "ge(2, 2)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "ge(1, 2)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ge('DEF', 'abc')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "ge('def', 'ABC')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ge('a', 'a')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "ge('a', 'b')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ge(4.5.6, 1.2.3)")); // version
                Assert.Equal(true, EvaluateAsBoolean(hc, "ge(1.2.3, 1.2.3)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "ge(1.2.3, 4.5.6)"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EvaluatesIn()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(true, false, false, true)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(true, false, true, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(true, true, false, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(true, true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(false, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "in(true, false, false, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "in(false, true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "in(true, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(2, 1, 2, 3)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "in(2, 1, 3, 4)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in('insensITIVE', 'other', 'INSENSitive')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "in('a', 'b', 'c')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(1.2.3, 1.1.1, 1.2.3)")); // version
                Assert.Equal(false, EvaluateAsBoolean(hc, "in(1.2.3, 4.5.6)"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void InCastsToMatchLeftSide()
        {
            using (var hc = new TestHostContext(this))
            {
                // Cast to bool.
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(true, 2)")); // number
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(false, 0)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(true, 'a')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(true, ' ')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(false, '')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(true, 1.2.3)")); // version
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(true, 0.0.0)"));

                // Cast to string.
                Assert.Equal(true, EvaluateAsBoolean(hc, "in('TRue', true)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "in('FALse', false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in('123456.789', 123456.789)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "in('123456.000', 123456.000)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in('1.2.3', 1.2.3)")); // version

                // Cast to number (best effort).
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(1, true)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(0, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "in(2, true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(123456.789, ' +123,456.7890 ')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(-123456.789, ' -123,456.7890 ')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(123000, ' 123,000.000 ')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(0, '')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "in(1, 'not a number')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "in(0, 'not a number')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "in(1.2, 1.2.0.0)")); // version

                // Cast to version (best effort).
                Assert.Equal(false, EvaluateAsBoolean(hc, "in(1.2.3, false)")); // bool
                Assert.Equal(false, EvaluateAsBoolean(hc, "in(1.2.3, true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "in(1.2.0, 1.2)")); // number
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(1.2.0, ' 1.2.0 ')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "in(1.2.0, '1.2')"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void InShortCircuitsAfterFirstMatch()
        {
            using (var hc = new TestHostContext(this))
            {
                // The gt function should never evaluate. It would would throw since 'not a number'
                // cannot be converted to a number.
                Assert.Equal(true, EvaluateAsBoolean(hc, "in(true, true, gt(1, 'not a number'))"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EvaluatesLessThan()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt(false, true)")); // bool
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(false, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(true, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(true, true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt(1, 2)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(1, 1)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(2, 1)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt('abc', 'DEF')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt('abc', 'DEF')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt('a', 'a')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt('b', 'a')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt(1.2.3, 4.5.6)")); // version
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(4.5.6, 1.2.3)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(1.2.3, 1.2.3)"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void LessThanCastsToMatchLeftSide()
        {
            using (var hc = new TestHostContext(this))
            {
                // Cast to bool.
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt(false, 1)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(false, 0)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt(false, 'a')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt(false, ' ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(false, '')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt(false, 1.2.3)")); // version
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt(false, 0.0.0)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(true, 1.2.3)"));

                // Cast to string.
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt('SSS', true)")); // bool
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt('UUU', true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt('123456.78', 123456.789)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt('123456.7899', 123456.789)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt('1.2.2', 1.2.3)")); // version
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt('1.2.4', 1.2.3)"));

                // Cast to number (or fails).
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt(0, true)")); // bool
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(0, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt(123456.788, ' +123,456.789 ')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(123456.7899, ' +123,456.789 ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(123456.789, ' +123,456.789 ')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "lt(-123456.7899, ' -123,456.789 ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(-123456.789, ' -123,456.789 ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(-123456.788, ' -123,456.789 ')"));
                try
                {
                    EvaluateAsBoolean(hc, "lt(1, 'not a number')");
                    throw new Exception("Should not reach here.");
                }
                catch (InvalidCastException ex)
                {
                    Assert.Equal("String", GetFromKind(ex));
                    Assert.Equal("Number", GetToKind(ex));
                    Assert.Equal("not a number", GetValue(ex));
                }

                try
                {
                    EvaluateAsBoolean(hc, "lt(1.2, 1.2.0.0)"); // version
                    throw new Exception("Should not reach here.");
                }
                catch (InvalidCastException ex)
                {
                    Assert.Equal("Version", GetFromKind(ex));
                    Assert.Equal("Number", GetToKind(ex));
                    Assert.Equal(new Version("1.2.0.0"), GetValue(ex));
                }

                // Cast to version (or fails).
                try
                {
                    EvaluateAsBoolean(hc, "lt(1.2.3, false)"); // bool
                    throw new Exception("Should not reach here.");
                }
                catch (InvalidCastException ex)
                {
                    Assert.Equal("Boolean", GetFromKind(ex));
                    Assert.Equal("Version", GetToKind(ex));
                    Assert.Equal(false, GetValue(ex));
                }

                Assert.Equal(true, EvaluateAsBoolean(hc, "lt(1.1.0, 1.2)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(1.3.0, 1.2)"));
                try
                {
                    EvaluateAsBoolean(hc, "lt(1.2.0, 2147483648.1)");
                    throw new Exception("Should not reach here.");
                }
                catch (InvalidCastException ex)
                {
                    Assert.Equal("Number", GetFromKind(ex));
                    Assert.Equal("Version", GetToKind(ex));
                    Assert.Equal(2147483648.1m, GetValue(ex));
                }

                Assert.Equal(true, EvaluateAsBoolean(hc, "lt(1.2.0, ' 1.2.1 ')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(1.2.1, ' 1.2.1 ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "lt(1.2.2, ' 1.2.1 ')"));
                try
                {
                    EvaluateAsBoolean(hc, "lt(1.2.1, 'not a version')");
                    throw new Exception("Should not reach here.");
                }
                catch (InvalidCastException ex)
                {
                    Assert.Equal("String", GetFromKind(ex));
                    Assert.Equal("Version", GetToKind(ex));
                    Assert.Equal("not a version", GetValue(ex));
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EvaluatesLessThanOrEqual()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "le(false, true)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "le(false, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "le(true, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "le(1, 2)")); // number
                Assert.Equal(true, EvaluateAsBoolean(hc, "le(2, 2)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "le(2, 1)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "le('abc', 'DEF')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "le('ABC', 'def')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "le('a', 'a')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "le('b', 'a')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "le(1.2.3, 4.5.6)")); // version
                Assert.Equal(true, EvaluateAsBoolean(hc, "le(1.2.3, 1.2.3)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "le(4.5.6, 1.2.3)"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EvaluatesNot()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "not(false)")); // bool
                Assert.Equal(false, EvaluateAsBoolean(hc, "not(true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "not(0)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "not(1)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "not('')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "not('a')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "not(' ')"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EvaluatesNotEqual()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(false, true)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(true, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne(false, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne(true, true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1, 2)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne(2, 2)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne('abc', 'def')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne('abcDEF', 'ABCdef')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2.3, 1.2.3.0)")); // version
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2.3, 4.5.6)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne(1.2.3, 1.2.3)"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void NotEqualCastsToMatchLeftSide()
        {
            using (var hc = new TestHostContext(this))
            {
                // Cast to bool.
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(false, 2)")); // number
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(true, 0)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(false, 'a')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(false, ' ')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(true, '')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(false, 1.2.3)")); // version
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(false, 0.0.0)"));

                // Cast to string.
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne('TRue', true)")); // bool
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne('FALse', false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne('123456.000', 123456.000)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne('123456.789', 123456.789)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne('1.2.3.0', 1.2.3)")); // version
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne('1.2.3', 1.2.3)"));

                // Cast to number (best effort).
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(2, true)")); // bool
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne(1, true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne(0, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne(123456.789, ' +123,456.7890 ')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne(-123456.789, ' -123,456.7890 ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne(123000, ' 123,000.000 ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne(0, '')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1, 'not a number')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(0, 'not a number')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2, 1.2.0.0)")); // version

                // Cast to version (best effort).
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2.3, false)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2.3, true)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2.0, 1.2)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "ne(1.2.0, ' 1.2.0 ')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "ne(1.2.0, '1.2')"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EvaluatesNotIn()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(true, false, false, false)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(true, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(false, true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(true, false, true, true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(true, false, true, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(true, true, false, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(true, true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(false, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(2, 1, 3, 4)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(2, 1, 2, 3)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn('a', 'b', 'c')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn('insensITIVE', 'INSENSitive')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(1.2.2, 1.1.1, 1.3.3)")); // version
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(1.2.2, 1.1.1, 1.2.2)"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void NotInCastsToMatchLeftSide()
        {
            using (var hc = new TestHostContext(this))
            {
                // Cast to bool.
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(true, 0)")); // number
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(false, 1)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(true, 1)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(false, 0)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(false, 'a')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(false, ' ')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(true, '')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(true, 'a')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(true, ' ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(false, '')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(false, 1.2.3)")); // version
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(true, 0.0.0)"));

                // Cast to string.
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn('TRue', false)")); // bool
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn('TRue', true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn('FALse', false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn('123456.000', 123456.000)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn('123456.789', 123456.789)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn('1.2.3', 1.2.4)")); // version
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn('1.2.3', 1.2.3)"));

                // Cast to number (best effort).
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(2, true)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(1, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(1, true)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(0, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(1, 'not a number')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(0, 'not a number')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(123456.789, ' +123,456.7890 ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(-123456.789, ' -123,456.7890 ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(123000, ' 123,000.000 ')"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(0, '')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(1.2, 1.2.0.0)")); // version

                // Cast to version (best effort).
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(1.2.3, false)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(1.2.0, 1.2)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(1.2.0, ' 1.2.0 ')")); // string
                Assert.Equal(true, EvaluateAsBoolean(hc, "notIn(1.2.0, '1.2')"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void NotInShortCircuitsAfterFirstMatch()
        {
            using (var hc = new TestHostContext(this))
            {
                // The gt function should never evaluate. It would would throw since 'not a number'
                // cannot be converted to a number.
                Assert.Equal(false, EvaluateAsBoolean(hc, "notIn(true, true, gt(1, 'not a number'))"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void EvaluatesOr()
        {
            using (var hc = new TestHostContext(this))
            {
                Assert.Equal(true, EvaluateAsBoolean(hc, "or(false, false, true)")); // bool
                Assert.Equal(true, EvaluateAsBoolean(hc, "or(false, true, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "or(true, false, false)"));
                Assert.Equal(false, EvaluateAsBoolean(hc, "or(false, false, false)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "or(false, 1)")); // number
                Assert.Equal(false, EvaluateAsBoolean(hc, "or(false, 0)"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "or(false, 'a')")); // string
                Assert.Equal(false, EvaluateAsBoolean(hc, "or(false, '')"));
                Assert.Equal(true, EvaluateAsBoolean(hc, "or(false, 1.2.3)")); // version
                Assert.Equal(true, EvaluateAsBoolean(hc, "or(false, 0.0.0)"));
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void OrShortCircuitsAfterFirstTrue()
        {
            using (var hc = new TestHostContext(this))
            {
                // The gt function should never evaluate. It would would throw since 'not a number'
                // cannot be converted to a number.
                Assert.Equal(true, EvaluateAsBoolean(hc, "or(true, gt(1, 'not a number'))"));
            }
        }

        ////////////////////////////////////////////////////////////////////////////////
        // Extension functions
        ////////////////////////////////////////////////////////////////////////////////
        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ExtensionReceivesState()
        {
            using (var hc = new TestHostContext(this))
            {
                try
                {
                    EvaluateAsBoolean(hc, "eq(1.2, 3.4a)");
                    throw new Exception("Should not reach here.");
                }
                catch (ParseException ex)
                {
                    Assert.Equal("UnrecognizedValue", GetKind(ex));
                    Assert.Equal("3.4a", GetRawToken(ex));
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////
        // Parse exceptions
        ////////////////////////////////////////////////////////////////////////////////
        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ThrowsWhenInvalidNumber()
        {
            using (var hc = new TestHostContext(this))
            {
                try
                {
                    EvaluateAsBoolean(hc, "eq(1.2, 3.4a)");
                    throw new Exception("Should not reach here.");
                }
                catch (ParseException ex)
                {
                    Assert.Equal("UnrecognizedValue", GetKind(ex));
                    Assert.Equal("3.4a", GetRawToken(ex));
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ThrowsWhenInvalidVersion()
        {
            using (var hc = new TestHostContext(this))
            {
                try
                {
                    EvaluateAsBoolean(hc, "eq(1.2.3, 4.5.6.7a)");
                    throw new Exception("Should not reach here.");
                }
                catch (ParseException ex)
                {
                    Assert.Equal("UnrecognizedValue", GetKind(ex));
                    Assert.Equal("4.5.6.7a", GetRawToken(ex));
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ThrowsWhenInvalidString()
        {
            using (var hc = new TestHostContext(this))
            {
                try
                {
                    EvaluateAsBoolean(hc, "eq('hello', 'unterminated-string)");
                    throw new Exception("Should not reach here.");
                }
                catch (ParseException ex)
                {
                    Assert.Equal("UnrecognizedValue", GetKind(ex));
                    Assert.Equal("'unterminated-string)", GetRawToken(ex));
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ThrowsWhenUnclosedFunction()
        {
            using (var hc = new TestHostContext(this))
            {
                try
                {
                    EvaluateAsBoolean(hc, "eq(1,2");
                    throw new Exception("Should not reach here.");
                }
                catch (ParseException ex)
                {
                    Assert.Equal("UnclosedFunction", GetKind(ex));
                    Assert.Equal("eq", GetRawToken(ex));
                }
            }
        }

        [Fact]
        [Trait("Level", "L0")]
        [Trait("Category", "Common")]
        public void ThrowsWhenExpectedStartParameter()
        {
            using (var hc = new TestHostContext(this))
            {
                try
                {
                    EvaluateAsBoolean(hc, "not(eq 1,2)");
                    throw new Exception("Should not reach here.");
                }
                catch (ParseException ex)
                {
                    Assert.Equal("ExpectedStartParameter", GetKind(ex));
                    Assert.Equal("eq", GetRawToken(ex));
                }
            }
        }

        private static bool EvaluateAsBoolean(IHostContext hostContext, string expression, IEnumerable<IExtensionInfo> extensions = null, object state = null)
        {
            var parser = new Parser();
            Node node = parser.CreateTree(expression, new TraceWriter(hostContext), extensions);
            var evaluationContext = new EvaluationContext(new TraceWriter(hostContext), state);
            return node.EvaluateBoolean(evaluationContext);
        }

        private static string GetFromKind(InvalidCastException ex)
        {
            return (ex.GetType().GetTypeInfo().GetProperty("FromKind", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ex) as object).ToString();
        }

        private static string GetToKind(InvalidCastException ex)
        {
            return (ex.GetType().GetTypeInfo().GetProperty("ToKind", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ex) as object).ToString();
        }

        private static object GetValue(InvalidCastException ex)
        {
            return ex.GetType().GetTypeInfo().GetProperty("Value", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ex) as object;
        }

        private static string GetKind(ParseException ex)
        {
            return (ex.GetType().GetTypeInfo().GetProperty("Kind", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ex) as object).ToString();
        }

        private static string GetRawToken(ParseException ex)
        {
            return ex.GetType().GetTypeInfo().GetProperty("RawToken", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ex) as string;
        }

        private sealed class TraceWriter : ITraceWriter
        {
            private readonly IHostContext _context;
            private readonly Tracing _trace;

            public TraceWriter(IHostContext context)
            {
                _context = context;
                _trace = context.GetTrace("ExpressionManager");
            }

            public void Info(string message)
            {
                _trace.Info(message);
            }

            public void Verbose(string message)
            {
                _trace.Verbose(message);
            }
        }
    }
}
