$(function() {

    var fileServer = "/GetFileList";

    $( ".tag_collection *" ).draggable({
        containment: "body",
        revert: true
    });
    
    $( "#tag_and" ).droppable({
        accept: ".tag_collection *",
        drop: function(event, ui){ addToCtc("AND",ui.draggable[0].innerText); },
        tolerance: "pointer"
    });    
    
    $( "#tag_or" ).droppable({
        accept: ".tag_collection *",
        drop: function(event, ui){ addToCtc("OR",ui.draggable[0].innerText); },
        tolerance: "pointer"
    });
    
    function runDaemon() {
        $.ajax({
          dataType: "json",
          url: fileServer,
          success: function(data) {
            if( files != data ) {
                alert("server content changed");
            }
            files = data.items;
            refreshFiles();
            setTimeout(runDaemon,3000);
          },
          error: function(a,b,c) {
            $( "#connect-dialog-form" ).dialog("open");
          },
        });
    }
    
    runDaemon();
    
    files = [];
    
    topTags = [];
    
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
               "<tr><td>" + this.name + "</td>" 
               + "<td>" + sizeToText(this.size) + "</td></tr>" );
        });
    }
    
    function sizeToText(size) {
        if(size <= 1024) return size + " B";
        if(size <= 1024*1024) return size/1024 + " KB";
        if(size <= 1024*1024*1024) return size/1024/1024 + " MB";
        return size/1024/1024/1024 + " GB" ;
    }
    
    function refreshCTC() {
        $("#navlist .elem").remove();
        $("#navlist .filtering").after(getCTCHeader(currTagCombination));
    }
    
    function getCTCHeader(ctc) {
        if($.type(ctc) == "string") return "<li class='elem'><a href='#'>" + ctc + "</a></li>";
        else {
            if(ctc.elems.length == 0)
                if(ctc.type == "AND") return "ALL";
                else return "NONE";
            else return ctc.elems.map(getCTCHeader).join(" <li class='elem'>"+ctc.type+"</li> ");
        }
    }
    
    // tag selection representation:
    // TS ::= { type: "OR", elems: [TS*] } | { type: "AND", elems: [TS*] } | '"'tagname'"'
    currTagCombination = { type: "AND", elems: [] };
    
    function evalTagMatching(ctc,file) {
        if($.type(ctc) == "string") {
            for( var i = 0; i < file.tags.length; i++ ) {
                if (ctc == file.tags[i])
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
    
});
