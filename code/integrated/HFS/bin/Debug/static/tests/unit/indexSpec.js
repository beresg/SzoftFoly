describe("Spec collection for index.js", function () {
	
	xdescribe("global variables", function () {
		it("should been defined and have an initial value", function () {
			expect(fileServer).toEqual("/GetFileList");
			expect(fileRepo).toEqual("/GetFile");
			expect(filesRepo).toEqual("/GetFiles");
		});
	});
	
	describe("sizeToText", function () {
		it("should give a appropiate postfix to file size", function () {
			var result = utils.sizeToText(1000);
			expect(result[result.length - 1]).toEqual("B");
			
			result = utils.sizeToText(1024*1024);
			expect(result[result.length - 2]).toEqual("K");
			expect(result[result.length - 1]).toEqual("B");
			
			result = utils.sizeToText(1024*1024*1024);
			expect(result[result.length - 2]).toEqual("M");
			expect(result[result.length - 1]).toEqual("B");
			
			result = utils.sizeToText(1024*1024*1024+1);
			expect(result[result.length - 2]).toEqual("G");
			expect(result[result.length - 1]).toEqual("B");
		});
	});
	
});