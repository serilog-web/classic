using Xunit;

// we rely on static data in the tests
// by default, Xunit runs tests of 2 different test classes in parallel
// we don't want that because setting static values in one test impacts the other test
// setting CollectionBehavior.CollectionPerAssembly means that xunit
// will consider all tests in this assembly to be in the same collection
// and therefore they should not be run in parallel with each other
[assembly:CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
