$(function() {

    var fileServer = "/GetFileList";
    var fileRepo = "/GetFile";
    var filesRepo = "/GetFiles";
    
    $("#header").click(function(){
        currTagCombination = { type: "AND", elems: [] };
        refreshFiles();
        refreshCTC();
    });
    
    $( "#tag_and" ).droppable({
        accept: ".tag_collection *",
        drop: function(event, ui){ 
          addToCtc("AND",ui.draggable[0].innerText); 
        },
        tolerance: "pointer"
    });    
    
    $( "#tag_or" ).droppable({
        accept: ".tag_collection *",
        drop: function(event, ui){ 
          addToCtc("OR",ui.draggable[0].innerText); 
        },
        tolerance: "pointer"
    });
    
    function runDaemon() {
        $.ajax({
          dataType: "json",
          url: fileServer,
          success: function(data) {
            files = data.items;
            refreshFiles();
            refreshTags();
            setTimeout(runDaemon,3000);
          },
          error: function(a,b,c) {
            $( "#connect-dialog-form" ).dialog("open");
          },
        });
    }
    
    runDaemon();
    
    files = [];
    
    tags = [];
    
    function addToCtc(type,tag) {
        if (currTagCombination.elems.length == 0) {
            // ALL OR sth = ALL, NONE AND sth = NONE
            if (type != currTagCombination.type) return; 
            // ALL AND sth = sth, NONE OR sth = sth
            else currTagCombination = { type: type, elems: [tag] };
        } else if (currTagCombination.type == type) {
            // sth AND sth = sth, sth OR sth = sth
            if ($.inArray(tag, currTagCombination.elems) >= 0) return;
            currTagCombination.elems.push(tag);
        } else {
            currTagCombination = {type:type, elems:[currTagCombination, tag]};
        }
        refreshFiles();
        refreshCTC();
    }
    
    function refreshFiles() {
        var matchingFiles = [];
        $.each(files, function(i,file){
            if(evalTagMatching(currTagCombination,file)) {
                matchingFiles.push(file);
            }
        });
        $( "#file_list tbody" ).empty();
        $.each(matchingFiles, function() {
            $( "#file_list tbody" ).append( 
               "<tr><td><a target='_blank' href='"+ fileRepo + "/" + this.id +"'>" + this.name + "</a></td>" 
               + "<td>" + sizeToText(this.size) + "</td></tr>" );
        });
    }
    
    function refreshTags() {
        $.each(files, function(i,file) {
            $.each(file.labels, function(j,label){
                if($.inArray(label, tags) == (-1)) {
                    tags.push(label);
                    var appended = $("<li class='tag'>" + label + "</li>");
                    appended.dblclick(function(e) {
                        currTagCombination = { type: "AND", elems: [label] };
                        refreshFiles();
                        refreshCTC();
                        if(e.shiftKey) {
                            window.location.href = filesRepo + "/" + label;
                        }
                    }).draggable({
                        containment: "body",
                        revert: true,
                        delay: 100,
                    });
                    $( "#all_time_top_tags" ).append(appended);
                }
            });
        });
    }
    
    function sizeToText(size) {
        if(size <= 1024) return size + " B";
        if(size <= 1024*1024) return (size/1024).toFixed(2) + " KB";
        if(size <= 1024*1024*1024) return (size/1024/1024).toFixed(2) + " MB";
        return (size/1024/1024/1024).toFixed(2) + " GB" ;
    }
    
    function refreshCTC() {
        $("#navlist .elem").remove();
        $("#navlist .filtering").after(getCTCHeader(currTagCombination));
    }
    
    function getCTCHeader(ctc) {
        if($.type(ctc) == "string") return "<li class='elem'><a href='#'>" + ctc + "</a></li>";
        else {
            if(ctc.elems.length == 0)
                if(ctc.type == "AND") return "<li class='elem'><a href='#'>ALL</a></li>";
                else return "<li class='elem'><a href='#'>NONE</a></li>";
            else return ctc.elems.map(getCTCHeader).join(" <li class='elem'>"+ctc.type+"</li> ");
        }
    }
    
    // tag selection representation:
    // TS ::= { type: "OR", elems: [TS*] } | { type: "AND", elems: [TS*] } | '"'tagname'"'
    currTagCombination = { type: "AND", elems: [] };
    
    function evalTagMatching(ctc,file) {
        if($.type(ctc) == "string") {
            for( var i = 0; i < file.labels.length; i++ ) {
                if (ctc == file.labels[i])
                    return true;
            }
            return false;
        } else { 
            if(ctc.type == "OR") {
                for( var i = 0; i < ctc.elems.length; i++ ) {
                    if (evalTagMatching(ctc.elems[i], file)) 
                        return true;
                }
                return false;
            }
            if(ctc.type == "AND") {
                for( var i = 0; i < ctc.elems.length; i++ ) {
                    if (!evalTagMatching(ctc.elems[i], file)) 
                        return false;
                }
                return true;
            }
        }
    }
            
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
        setTimeout(runDaemon,3000);
      }
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
    
    
});
