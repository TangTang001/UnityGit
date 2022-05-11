mergeInto(LibraryManager.library, {  
  sendClickSignal:function(str){
	var str1=Pointer_stringify(str);
	clickEvent(str1);	
  }
});