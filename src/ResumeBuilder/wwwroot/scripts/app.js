window.setSource = async (elementId, stream) => {
  const arrayBuffer = await stream.arrayBuffer();
  let blobOptions = {};
  blobOptions['type'] = "text/html";
  const blob = new Blob([arrayBuffer], blobOptions);
  const url = URL.createObjectURL(blob);
  const element = document.getElementById(elementId);
  element.onload = () => {
    URL.revokeObjectURL(url);
  }
  element.src = url;
}

window.split = async () => {  
    var sizes = localStorage.getItem('split-sizes')

    if (sizes) {
        sizes = JSON.parse(sizes)
    } else {
        sizes = [50, 50]
    }    

    Split(["#left", "#right"], {
        gutterSize: 6,
        sizes: sizes,
        onDragEnd: function (sizes) {
            localStorage.setItem('split-sizes', JSON.stringify(sizes))
        },        
    });  
}