(function ($) {

	$("#header").click(function(){
		currTagCombination = { type: "AND", elems: [] };
		utils.refreshFiles();
		utils.refreshCTC();
	});

	$( "#tag_and" ).droppable({
		accept: ".tag_collection *",
		drop: function(event, ui){ 
		  utils.addToCtc("AND",ui.draggable[0].innerText); 
		},
		tolerance: "pointer"
	});    
	
	$( "#tag_or" ).droppable({
		accept: ".tag_collection *",
		drop: function(event, ui){ 
		  utils.addToCtc("OR",ui.draggable[0].innerText); 
		},
		tolerance: "pointer"
	});
	
	$( "#stylesheet-changer .stylesheet-opt" ).click(function(){
		$( "#main-stylesheet" ).attr("href",$(this).data("css-file"));
	});
	
	$( "#sendFileButton" )
	  .button()
	  .click(function( event ) {
			var formData = new FormData($('#fileInputForm')[0]);
			$.ajax({
				url: '/PostTest',  //Server script to process data
				type: 'POST',
				//success: alert("success"),
				//error: alert("error"),
				// Form data
				data: formData,
				//Options to tell jQuery not to process data or worry about content-type.
				cache: false,
				contentType: false,
				processData: false
			});
			event.preventDefault();
	});
	
    $( "#chatBox" ).dialog({ 
        height : 200,
        position : {
            at: "right bottom", of: $("#page")    
        }
    });
    
	$( "#connect-dialog-form" ).dialog({
	  autoOpen: false,
	  width: 350,
	  modal: true,
	  buttons: {
		"Connect": function() {
		  fileServer = $("#url").val();
		  $(this).dialog("close");
		}
	  },
	  close: function() {
		setTimeout(utils.runDaemon,3000);
	  }
	});
	
    $( "#chatBoxInput" ).keypress(function( event ) {
        if(event.which == 13) { // enter is pressed
            $.post({
                url: "/SendChatMsg",
                data: { text: $(this).val() }
            });
            $(this).val("");
        }
    });
	
}(jQuery));