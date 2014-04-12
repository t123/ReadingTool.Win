using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Ninject;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Web
{
    public class SyncController : ApiController
    {
        private TermService _termService;

        public SyncController()
        {
            _termService = App.Container.Get<TermService>();
        }
        public IEnumerable<Term> GeTerms()
        {
            return _termService.FindAll();
        }

        public Term GeTerm(long id)
        {
            return _termService.FindOne(id);
        }
    }
}
