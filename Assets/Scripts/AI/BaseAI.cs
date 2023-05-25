using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
/*                                                                                        
         &EQSDDDDDOSF#                                                                                                  
       EQDDDDDDDDDDDDDSF                                                                       QDOD#                    
     EDDDDDDDDDDDDDDDDDDOE                                                                     QDDD#                    
    QDDDDDDDQGF#GGSODDDDCS                                                                     QDDD#                    
  &ODDDDCSF&        EQDDG                                                                      QDDD#                    
 &SDDDDDG             EE                                                                       QDDD#                    
 QCDDDDF                                                                                       QDDD#                    
#DDDDCF                                                                                        QDDD#          &         
GDDDDQ                              &FSSSSSSQE           #QJJJJJJCG&           ESSSSSSF&EGGGE  QDDD#     &FQSSOSSG#     
ODDDD#                            &QODOOODOODOSF       &Di@@@@@@@@@iO        GODDDDDDDDDDCDCQ  QDDD#    GODOOOOOODDQ&   
DDDDO          EGGGGGGGGGGGGGG   EODOOODOOODOOODQ&    EI@@@@@@i@@@@@@J#    &SCDDDDCDDDDDDDDDQ  QDDD#  #SDOOODOOODOODS#  
DDDDS          GCDDDDDDDDDDDCD  EDOOODSF&&#GOOOODS   E@@@@@IOE&EDi@@@@i#  &SCDDDOGF&&GODDDDDQ  QDDD#  SDOODSF#&&FSOOOO# 
DDDDO          GCDDDDDDDDDDDDO &OOOOOF      #SOOODF  J@@@@D&     #C@@@@C  GCDDDS&      QDDDDQ  QDDD# FDOOOQ&    #SOOODS 
ODDDDE         &EEEEEEEEEODDDO FDOOOF        &SOOOO F@@@@D         J@@@@E ODDDD&        ODDDQ  QDDD# SOOOO   EGSOOOODOS&
GDDDDS                  &DDDDQ QDOOO&         GDOOO&S@@@@F         Q@@@@F#DDDCG         FDDDQ  QDDD# OOOOQEQOODOODOSG#  
&ODDDDF                 QDDDCG GDOOD&         GDOOO Q@@@iE         Q@@@@F#DDDCF         #DDDQ  QDDD# SOOOODOOODOSF#     
 GDDDDDG               EDDDDO& QDOOO&         QDOOO&S@@@@G         S@@@@F#CDDCG         QDDDQ  QDDD# OOOOOOOOQE#        
  GDDDDCS&            GDDDDDF  EDOOOQ        #OOOOS E@@@@I        #i@@@@E ODDDD#       &DDDDQ  QDDD# QDOOOQ#       #&   
   QDDDDDOG#       &FSDDDDDG    SOOOOG&     ESOOODE  I@@@@D#     GJ@@@.O  FCDDDDF     EODDDDQ  QDDD# EDOOOSE     &GDOF& 
    FDDDDDDDOSSSSSODDDDDDDE     #OOOODOGEEFSOOOODG   Ei@@@@iCQQQC@@@@@I#   QDDDDDSFFFQDDDDDDQ  QDDD#  GDOOOOQFEEGSDOODS 
     #QDDDDDDDDDDDDDDDDDS&       EODOOODDDDOOOODQ     EI@@@@@@@@@@@@@J#     QDDDDDDDDDDDDDDDQ  QDDD#   FODOOODDDDOOODS# 
       #GODDCDDDDDDCDDQE          &FSDDDOOODDOQ#       &GI@@@@@@@@@JF        #SDCDDDDDDSODDDQ  QDDD#    #QODDOOOODOSF   
          #FFSSSSSGF#&               EFQSSSGF&            FSDJJJOSE            #FGSSOGE SDDDQ  &&&&       &EFQSSQF#     
                                                                             EE         ODDDQ                           
                                                                          #GODCF       GDDDDF                           
                                                                          #DDDDDQ#  &#QDDDDO                            
                                                                           EDDDDDDOOODDDDDD#                            
                                                                            ESDDDDDDDDDDDS#                             
                                                                              EQODDDDDOQE                               
                                                                                 #EEE&                                  
 */
public abstract class BaseAI : MonoBehaviour
{
    protected BaseControl control;
    public BehaviorTree _tree;

    protected virtual void Awake()
    {
        TryGetComponent(out control);

    }

    public TaskStatus IsGameOver()
    {
        if (GameManager.Instance.IsGameOver)
        {
            control.SetStop();
            return TaskStatus.Failure;
        }
        return TaskStatus.Success;
    }

    /// <summary>
    /// 闲逛
    /// </summary>
    public TaskStatus HangOut()
    {
        if (control.IsStop == false)
            return TaskStatus.Success;
        var dir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        var nowDir = dir.normalized;
        dir = nowDir * Random.Range(.3f, .7f);
        control.Move(transform.position + dir);
        return TaskStatus.Success;
    }

    protected virtual void Update()
    {
        if (control.IsLocalPlayer)
            return;
        _tree.Tick();
    }

}
