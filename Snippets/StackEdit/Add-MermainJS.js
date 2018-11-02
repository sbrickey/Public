// add CSS file
    var head  = document.getElementsByTagName('head')[0];
    var link  = document.createElement('link');
    link.rel  = 'stylesheet';
    link.type = 'text/css';
    link.href = 'https://cdn.rawgit.com/knsv/mermaid/6.0.0/dist/mermaid.css';
    link.media = 'all';
    head.appendChild(link);

$('#preview-contents .language-mermaid').addClass("mermaid");
require.config({ paths: { "mermaid" : "https://cdn.rawgit.com/knsv/mermaid/6.0.0/dist/mermaid" } });
require( ["mermaid"], function ($) { mermaid.initialize({ startOnLoad:false, cloneCssStyles: true }); mermaid.init(); } );

userCustom.onPreviewFinished = function() {
    $('#preview-contents .language-mermaid').each(function(){
        var sectionName = $(this).parent().parent().attr('id');
        var sectionId = sectionName.substring(sectionName.lastIndexOf("-")+1);
        var renderId = "mermaidGraph-" + sectionId;
        $(this).html(function(index, html){
            return mermaid.render(renderId, $.parseHTML("<div>"+html+"</div>")[0].innerText)
        })
    });
};
