describe("Spec collection for utils.js", function () {
	
	var ctc1 = { type: "AND", elems: ["zene", "rock"]};
	var ctc2 = { type: "OR", elems: ["zene", "rock"]};
	var ctc3 = "zene";
	
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
	
	describe("refreshMessages ", function () {
		
		beforeEach(function () {
			jasmine.getFixtures().fixturesPath = 'base/tests/fixtures/';
		});
		
		it("should render its parameters to the page", function () {
			var	messages = [{
				sender: "valaki",
				text: "szoveg"
			},
			{
				sender: "valaki2",
				text: "szoveg2"
			}],
				firstLine = "<div>valaki: szoveg</div>";
			loadFixtures('chatBoxFixture.html');
			utils.refreshMessages(messages);
			expect($("#chatBoxMessages div").length).toEqual(2);
		});
	});
	
	describe("evalTagMatching", function () {
		var file = {};
		
		beforeEach(function () {
			file.labels = ["valami","tag","rock"];
		});
	
		it("should return true if the tags on the files matches the current tag selection", function () {
			var result = utils.evalTagMatching(ctc1, file);
			expect(result).toBe(false);
			result = utils.evalTagMatching(ctc2, file);
			expect(result).toBe(true);
			file.labels=["zene", "rock"];
			result = utils.evalTagMatching(ctc1, file);
			expect(result).toBe(true);
			file.labels=["rap"];
			result = utils.evalTagMatching(ctc2, file);
			expect(result).toBe(false);
		});
	});
	
	describe("getCTCHeader", function () {
		it("should render a simple list if a tag is a simple string", function () {
			var result = utils.getCTCHeader("rock");
			expect(result).toBe( "<li class='elem'><a href='#'>rock</a></li>");
		});
		
		it("should render a simple list with ALL or NONE when its a complex tag combination and there is no tags to combine", function () {
			var result = utils.getCTCHeader({type: "AND" ,elems: []});
			expect(result).toBe( "<li class='elem'><a href='#'>ALL</a></li>");
			result = utils.getCTCHeader({type: "OR" ,elems: []});
			expect(result).toBe( "<li class='elem'><a href='#'>NONE</a></li>");
		});
		
		it("should render the current logical expressiopn when ctc is complex", function () {
			var result = utils.getCTCHeader(ctc1);
			expect(result).toBe("<li class='elem'><a href='#'>zene</a></li> <li class='elem'>AND</li> <li class='elem'><a href='#'>rock</a></li>");
		});
	});
	
	describe("refreshCTC", function () {
		
		beforeEach(function () {
			jasmine.getFixtures().fixturesPath = 'base/tests/fixtures/';
		});
	
		it("should render the current tag composition list according to the current CTC", function () {
			loadFixtures('ctcHeaderFixture.html');
			utils.currTagCombination = ctc1;
			utils.refreshCTC();
			expect($("#navlist").length).toEqual(1);
		});
	});
	
	describe("addToCtc", function () {
		
		beforeEach(function () {
			utils.currTagCombination = { type: "AND", elems: [] };
		});
	
		xit("should call refresh methods", function () {
			var utilSpy = jasmine.createSpyObj('utils', ['refreshFiles', 'refreshCTC', 'addToCtc']);
			utilSpy.addToCtc("OR", "haha");
			expect(utilSpy.refreshFiles).toHaveBeenCalled();
		});
		
		it("should not modify the type if there are no tags and the new type if differs then the default", function () {
			//utils.addToCtc("OR", "rock");
			utils.addToCtc("AND", "rock");
			expect(utils.currTagCombination.type === "OR").toBe(false);
			
			//utils.currTagCombination.elems.push("rock");
			utils.addToCtc("AND", "zene");
			console.log(utils.currTagCombination.elems);
			expect(utils.currTagCombination.elems.length).toEqual(2);
			//expect(utils.currTagCombination.elems[0]).toEqual("rock");
		});
	});
});